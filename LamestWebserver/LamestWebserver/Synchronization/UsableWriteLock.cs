﻿using LamestWebserver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LamestWebserver.Synchronization
{
    /// <summary>
    /// A safe and fast way to read and write from shared ressources without blocking everything.
    /// </summary>
    public class UsableWriteLock
    {
        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly ID ID = new ID();
        private UsableWriteLockDisposable_read _readDisposable;
        private UsableWriteLockDisposable_write _writeDisposable;

        /// <summary>
        /// Locks the WriteLock for reading
        /// </summary>
        /// <returns>An IDisposable Object to be used in a using statement</returns>
        public UsableWriteLockDisposable_read LockRead()
        {
            rwLock.EnterReadLock();

            if (_readDisposable == null)
                _readDisposable = new UsableWriteLockDisposable_read(this);

            return _readDisposable;
        }

        /// <summary>
        /// Locks the WriteLock for Writing
        /// </summary>
        /// <returns>An IDisposable Object to be used in a using statement</returns>
        public UsableWriteLockDisposable_write LockWrite()
        {
            rwLock.EnterWriteLock();

            if (_writeDisposable == null)
                _writeDisposable = new UsableWriteLockDisposable_write(this);

            return _writeDisposable;
        }

        /// <summary>
        /// Locks a couple of UsableWriteLocks for Reading in order to prevent deadlocks.
        /// </summary>
        /// <param name="locks">the WriteLocks to lock</param>
        /// <returns>a multidisposer to release the locks in opposite order</returns>
        public static MultiDisposer LockRead(params UsableWriteLock[] locks)
        {
            IDisposable[] disposables = new IDisposable[locks.Length];
            UsableWriteLock[] lockys = new UsableWriteLock[locks.Length];

            for (int i = 0; i < locks.Length; i++)
            {
                int currentIndex = -1;

                for (int j = 0; j < locks.Length; j++)
                {
                    if (lockys.Contains(locks[j]))
                        continue;

                    if (lockys[i] == null)
                        currentIndex = j;
                    else if (lockys[i].ID < locks[j].ID)
                        currentIndex = j;
                }

                lockys[i] = locks[currentIndex];
                disposables[i] = locks[currentIndex].LockRead();
            }

            return new MultiDisposer(disposables);
        }

        /// <summary>
        /// Locks a couple of UsableWriteLocks for Writing in order to prevent deadlocks.
        /// </summary>
        /// <param name="locks">the WriteLocks to lock</param>
        /// <returns>a multidisposer to release the locks in opposite order</returns>
        public static MultiDisposer LockWrite(params UsableWriteLock[] locks)
        {
            IDisposable[] disposables = new IDisposable[locks.Length];
            UsableWriteLock[] lockys = new UsableWriteLock[locks.Length];

            for (int i = 0; i < locks.Length; i++)
            {
                int currentIndex = -1;

                for (int j = 0; j < locks.Length; j++)
                {
                    if (lockys.Contains(locks[j]))
                        continue;

                    if (lockys[i] == null)
                        currentIndex = j;
                    else if (lockys[i].ID < locks[j].ID)
                        currentIndex = j;
                }

                lockys[i] = locks[currentIndex];
                disposables[i] = locks[currentIndex].LockWrite();
            }

            return new MultiDisposer(disposables);
        }

        /// <summary>
        /// A helper class to be used in a using statement
        /// </summary>
        public class UsableWriteLockDisposable_write : IDisposable
        {
            private UsableWriteLock writeLock;

            internal UsableWriteLockDisposable_write(UsableWriteLock writeLock)
            {
                this.writeLock = writeLock;
            }

            /// <summary>
            /// Releases the Semaphore
            /// </summary>
            public void Dispose()
            {
                writeLock.rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// A helper class to be used in a using statement
        /// </summary>
        public class UsableWriteLockDisposable_read : IDisposable
        {
            private readonly UsableWriteLock _writeLock;

            internal UsableWriteLockDisposable_read(UsableWriteLock writeLock)
            {
                this._writeLock = writeLock;
            }

            /// <summary>
            /// Releases the mutex and unsubscribes from the writeLock
            /// </summary>
            public void Dispose()
            {
                this._writeLock.rwLock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// A wrapper class for a writeLock to be used in a using statement
    /// </summary>
    public class UsableSemaphore : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Constructs a new UsableSemaphore. The Semaphore is not locked and will not be locked until you call Lock().
        /// </summary>
        /// <param name="semaphore">the writeLock</param>
        public UsableSemaphore(SemaphoreSlim semaphore)
        {
            this._semaphore = semaphore;
        }

        /// <summary>
        /// Locks this Semaphore.
        /// </summary>
        public void Lock()
        {
            this._semaphore.Wait();
        }

        /// <summary>
        /// Releases this Semaphore.
        /// </summary>
        public void Dispose()
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// A MultiDisposer disposes all given object on dispose.
    /// </summary>
    public class MultiDisposer : IDisposable
    {
        private readonly IDisposable[] _disposables;

        /// <summary>
        /// Creates a MultiDisposer.
        /// </summary>
        /// <param name="disposables">the IDisposable objects to dispose on dispose.</param>
        public MultiDisposer(params IDisposable[] disposables)
        {
            this._disposables = disposables;
        }

        /// <summary>
        /// Disposes all given disposing objects in oposite order.
        /// </summary>
        public void Dispose()
        {
            for (int i = _disposables.Length - 1; i >= 0; i--)
            {
                _disposables[i].Dispose();
            }
        }
    }

}
