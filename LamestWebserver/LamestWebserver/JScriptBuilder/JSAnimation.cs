﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamestWebserver.JScriptBuilder
{
    /// <summary>
    /// Contains basic functionality for Animating Elements
    /// </summary>
    public static class JSAnimation
    {
        /// <summary>
        /// Various types of animation types
        /// </summary>
        public enum JSAnimationType
        {
            /// <summary>
            /// linear function - constant speed
            /// </summary>
            Linear,

            /// <summary>
            /// Differencial function - decreasing speed
            /// </summary>
            Differencial,

            /// <summary>
            /// Quadratic function - increasing speed
            /// </summary>
            Quadreatic
        }

        /// <summary>
        /// Axis at which the animation is applied
        /// </summary>
        public enum JSAxis
        {
            /// <summary>
            /// X Axis
            /// </summary>
            X,

            /// <summary>
            /// Y Axis
            /// </summary>
            Y,

            /// <summary>
            /// Both Axis
            /// </summary>
            Both
        }

        /// <summary>
        /// Resizes an element until it fits the contents inside.
        /// </summary>
        /// <param name="element">the element</param>
        /// <param name="axis">the axis of the animation</param>
        /// <param name="animationType">the type of the function</param>
        /// <param name="speedFactor">the speed factor of the animation</param>
        /// <returns>the animation as function</returns>
        public static JSFunction ResizeElementToFit(IJSValue element, JSAxis axis = JSAxis.Y, JSAnimationType animationType = JSAnimationType.Differencial, int speedFactor = 100)
        {
            if (axis != JSAxis.Y || animationType != JSAnimationType.Differencial)
                throw new InvalidOperationException("Only Y Axis and Differencial supported yet.");
            
            return new JSInstantFunction(new JSValue("function changeheight(object, oldsize, newsize){var rem = newsize - oldsize;var speed = " + speedFactor + ".0;object.style.overflow = \"hidden\"; object.style.height = oldsize; function move() { var f = (rem) / speed; oldsize += f; object.style.height = oldsize + f; if (Math.abs(newsize - oldsize) < 0.1) { object.style.overflow = \"auto\"; object.style.height = newsize; clearInterval(interval0); } } var interval0 = setInterval(move, 10); } "
                + "var obj0 = " + element.getCode(SessionData.currentSessionData, CallingContext.Default) + " var oldsize = obj0.getBoundingClientRect().height;  obj0.style.overflow = \"auto\"; obj0.style.height = \"auto\"; changeheight(obj0, oldsize, obj0.getBoundingClientRect().height);"));
        }
        
        /// <summary>
        /// Resizes an element until it's size in the given axis is zero
        /// </summary>
        /// <param name="element">the element</param>
        /// <param name="axis">the axis of the animation</param>
        /// <param name="animationType">the type of the function</param>
        /// <param name="speedFactor">the speed factor of the animation</param>
        /// <returns>the animation as function</returns>
        public static JSFunction DecreaseElementToZero(IJSValue element, JSAxis axis = JSAxis.Y, JSAnimationType animationType = JSAnimationType.Differencial, int speedFactor = 100)
        {
            if (axis != JSAxis.Y || animationType != JSAnimationType.Differencial)
                throw new InvalidOperationException("Only Y Axis and Differencial supported yet.");

            return new JSInstantFunction(new JSValue("function changeheight(object, oldsize, newsize){var rem = newsize - oldsize;var speed = " + speedFactor + ".0;object.style.overflow = \"hidden\"; object.style.height = oldsize; function move() { var f = (rem) / speed; oldsize += f; object.style.height = oldsize + f; if (Math.abs(newsize - oldsize) < 0.1) { object.style.overflow = \"auto\"; object.style.height = newsize; clearInterval(interval0); } } var interval0 = setInterval(move, 10); } "
                + "var obj0 = " + element.getCode(SessionData.currentSessionData, CallingContext.Default) + " var oldsize = obj0.getBoundingClientRect().height;  obj0.style.overflow = \"auto\"; obj0.style.height = \"auto\"; changeheight(obj0, oldsize, 0);"));
        }
    }
}