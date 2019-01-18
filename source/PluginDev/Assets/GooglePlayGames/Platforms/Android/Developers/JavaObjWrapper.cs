// <copyright file="JavaObjWrapper.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

#if UNITY_ANDROID
namespace Google.Developers
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Java object wrapper.  This class encapsulates the the java object
    /// pointer and provides the functionality to call methods and properties.
    /// </summary>
    public  class JavaObjWrapper
    {
        /// <summary>
        /// The raw java object pointer.
        /// </summary>
        IntPtr raw;

        IntPtr cachedRawClass = IntPtr.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="Google.Developers.JavaObjWrapper"/> class.
        /// Does not create an instance of the java class.
        /// </summary>
        protected JavaObjWrapper()
        {
        }

        /// <summary>
        /// Creates a new instance of the given class by calling the
        /// no-arg constructor.
        /// </summary>
        /// <param name="clazzName">Clazz name.</param>
        public JavaObjWrapper(string clazzName)
        {
            this.raw = AndroidJNI.AllocObject(AndroidJNI.FindClass(clazzName));
        }

        /// <summary>
        /// Wraps the given pointer.
        /// </summary>
        /// <param name="rawObject">Raw object.</param>
        public JavaObjWrapper(IntPtr rawObject)
        {
            raw = rawObject;
        }

        /// <summary>
        /// Gets the raw object.
        /// </summary>
        /// <value>The raw object.</value>
        public IntPtr RawObject
        {
            get
            {
                return raw;
            }
        }

        public virtual IntPtr RawClass
        {
            get
            {
                if (cachedRawClass == IntPtr.Zero && raw != IntPtr.Zero)
                {
                    cachedRawClass = AndroidJNI.GetObjectClass(raw);
                }
                return cachedRawClass;
            }
        }

        /// <summary>
        /// Creates an instance of the java object.  The arguments are for
        /// the constructor.
        /// </summary>
        /// <param name="clazzName">Clazz name.</param>
        /// <param name="args">Arguments.</param>
        public void CreateInstance(string clazzName, params object[]  args)
        {
            // Can't create an instance if there is already a pointer.
            if (raw != IntPtr.Zero)
            {
                throw new Exception("Java object already set");
            }

            // TODO: use a specific signature. This could be problematic when
            // using arguments that are subclasses of the declared parameter types.
            IntPtr method = AndroidJNIHelper.GetConstructorID(RawClass, args);
            jvalue[] jArgs = ConstructArgArray(args);

            try 
            {
                // assign the raw object.
                raw = AndroidJNI.NewObject(RawClass, method, jArgs);
            }
            finally 
            {
                AndroidJNIHelper.DeleteJNIArgArray(args, jArgs);
            }
        }

        /// <summary>
        /// Constructs the argument array for calling a JNI method.
        ///
        /// This wraps the JNIHelper method so JavaObjWrapper arguments can be
        /// found and use the raw pointer.
        /// </summary>
        /// <returns>The argument array.</returns>
        /// <param name="theArgs">Arguments.</param>
        protected static jvalue[] ConstructArgArray(object[] theArgs)
        {

            object[] a = new object[theArgs.Length];
            for (int i = 0; i < theArgs.Length; i++)
            {
                if (theArgs[i] is JavaObjWrapper)
                {
                    a[i] = ((JavaObjWrapper)theArgs[i]).raw;
                }
                else
                {
                    a[i] = theArgs[i];
                }
            }

            jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(a);

            for (int i = 0; i < theArgs.Length; i++)
            {
                if (theArgs[i] is JavaObjWrapper)
                {
                    args[i].l = ((JavaObjWrapper)theArgs[i]).raw;
                }
                else if (theArgs[i] is JavaInterfaceProxy)
                {
                    IntPtr v = AndroidJNIHelper.CreateJavaProxy(
                        (AndroidJavaProxy)theArgs[i]);
                    args[i].l = v;
                }
            }

            if (args.Length == 1)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    Debug.Log("---- [" + i + "] -- " + args[i].l);
                }
            }
            return args;
        }

        /// <summary>
        /// Calls a static method with an object return type.
        /// </summary>
        /// <returns>The invoke call.</returns>
        /// <param name="type">Type.</param>
        /// <param name="name">Name.</param>
        /// <param name="sig">Sig.</param>
        /// <param name="args">Arguments.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T StaticInvokeObjectCall<T>(
            string type, string name, string sig, params object[] args)
        {
            IntPtr rawClass = AndroidJNI.FindClass(type);
            IntPtr method = AndroidJNI.GetStaticMethodID(rawClass, name, sig);
            jvalue[] jArgs = ConstructArgArray(args);
            
            try 
            {
                IntPtr val = AndroidJNI.CallStaticObjectMethod(rawClass, method, jArgs);
                ConstructorInfo c = typeof(T).GetConstructor(new Type[] { val.GetType() });
                if (c != null)
                {
                    return (T)c.Invoke(new object[] { val });
                }
                if (typeof(T).IsArray)
                {
                    // make an array
                    //TODO: handle arrays of objects
                    return AndroidJNIHelper.ConvertFromJNIArray<T>(val);
                }
                Debug.Log("Trying cast....");
                Type t = typeof(T);
                return (T)Marshal.PtrToStructure(val, t);
            }
            finally 
            {
                AndroidJNIHelper.DeleteJNIArgArray(args, jArgs);
            }
        }

        /// <summary>
        /// Invokes a static method with void return type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="name">Name.</param>
        /// <param name="sig">Sig.</param>
        /// <param name="args">Arguments.</param>
        public static void StaticInvokeCallVoid(
            string type, string name, string sig, params object[] args)
        {
            IntPtr rawClass = AndroidJNI.FindClass(type);
            IntPtr method = AndroidJNI.GetStaticMethodID(rawClass, name, sig);
            jvalue[] jArgs = ConstructArgArray(args);
            try 
            {
                AndroidJNI.CallStaticVoidMethod(rawClass, method, jArgs);
            }
            finally 
            {
                AndroidJNIHelper.DeleteJNIArgArray(args, jArgs);
            }
        }

        /// <summary>
        /// Gets the value of a static field returning an object.
        /// </summary>
        /// <returns>The static object field.</returns>
        /// <param name="clsName">Cls name.</param>
        /// <param name="name">Name.</param>
        /// <param name="sig">Sig.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetStaticObjectField<T>(string clsName, string name, string sig)
        {
            IntPtr rawClass = AndroidJNI.FindClass(clsName);
            IntPtr id = AndroidJNI.GetStaticFieldID(rawClass, name, sig);
            IntPtr val = AndroidJNI.GetStaticObjectField(rawClass, id);

            ConstructorInfo c = typeof(T).GetConstructor(new Type[] { val.GetType() });
            if (c != null)
            {
                return (T)c.Invoke(new object[] { val });
            }

            Type t = typeof(T);
            return (T)Marshal.PtrToStructure(val, t);
        }

        /// <summary>
        /// Gets the value of a static int field.
        /// </summary>
        /// <returns>The static int field.</returns>
        /// <param name="clsName">Cls name.</param>
        /// <param name="name">Name.</param>
        public static int GetStaticIntField(string clsName, string name)
        {
            IntPtr rawClass = AndroidJNI.FindClass(clsName);
            IntPtr method = AndroidJNI.GetStaticFieldID(rawClass, name, "I");
            return AndroidJNI.GetStaticIntField(rawClass, method);
        }

        /// <summary>
        /// Gets the value of a static string field.
        /// </summary>
        /// <returns>The static string field.</returns>
        /// <param name="clsName">Cls name.</param>
        /// <param name="name">Name.</param>
        public static string GetStaticStringField(string clsName, string name)
        {
            IntPtr rawClass = AndroidJNI.FindClass(clsName);
            IntPtr method = AndroidJNI.GetStaticFieldID(rawClass, name, "Ljava/lang/String;");
            return AndroidJNI.GetStaticStringField(rawClass, method);
        }

        /// <summary>
        /// Gets the value of a static float field.
        /// </summary>
        /// <returns>The static float field.</returns>
        /// <param name="clsName">Cls name.</param>
        /// <param name="name">Name.</param>
        public static float GetStaticFloatField(string clsName, string name)
        {
            IntPtr rawClass = AndroidJNI.FindClass(clsName);
            IntPtr method = AndroidJNI.GetStaticFieldID(rawClass, name, "F");
            return AndroidJNI.GetStaticFloatField(rawClass, method);
        }

        /// <summary>
        /// Calls a non-static method with a void return type.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="sig">Sig.</param>
        /// <param name="args">Arguments.</param>
        public void InvokeCallVoid(string name, string sig, params object[] args)
        {
            IntPtr method = AndroidJNI.GetMethodID(RawClass, name, sig);

            jvalue[] jArgs = ConstructArgArray(args);
            try 
            {
                AndroidJNI.CallVoidMethod(raw, method, jArgs);
            }
            finally 
            {
                AndroidJNIHelper.DeleteJNIArgArray(args, jArgs);
            }
        }

        public T InvokeCall<T>(string name, string sig, params object[] args)
        {
            Type t = typeof(T);
            IntPtr method = AndroidJNI.GetMethodID(RawClass, name, sig);

            if (method == IntPtr.Zero) {
                Debug.LogError("Cannot get method for " + name);
                throw new Exception("Cannot get method for " + name);
            }

            jvalue[] jArgs = ConstructArgArray(args);

            try 
            {
                if (t == typeof(bool))
                {
                    return (T)(object)AndroidJNI.CallBooleanMethod(raw, method, jArgs);
                }
                else if (t == typeof(string))
                {
                    return (T)(object)AndroidJNI.CallStringMethod(raw, method, jArgs);
                }
                else if (t == typeof(int))
                {
                    return (T)(object)AndroidJNI.CallIntMethod(raw, method, jArgs);
                }
                else if (t == typeof(float))
                {
                    return (T)(object)AndroidJNI.CallFloatMethod(raw, method, jArgs);
                }
                else if (t == typeof(double))
                {
                    return (T)(object)AndroidJNI.CallDoubleMethod(raw, method, jArgs);
                }
                else if (t == typeof(byte))
                {
                    return (T)(object)AndroidJNI.CallByteMethod(raw, method, jArgs);
                }
                else if (t == typeof(char))
                {
                    return (T)(object)AndroidJNI.CallCharMethod(raw, method, jArgs);
                }
                else if (t == typeof(long))
                {
                    return (T)(object)AndroidJNI.CallLongMethod(raw, method, jArgs);
                }
                else if (t == typeof(short))
                {
                    return (T)(object)AndroidJNI.CallShortMethod(raw, method, jArgs);
                }
                else if (t == typeof(IntPtr))
                {
                    return (T)(object)AndroidJNI.CallObjectMethod(raw, method, jArgs);
                }
                else
                {
                    return InvokeObjectCall<T>(name, sig, args);
                }
            }
            finally 
            {
                AndroidJNIHelper.DeleteJNIArgArray(args, jArgs);
            } 
        }

        public static T StaticInvokeCall<T>(string type, string name, string sig, params object[] args)
        {
            Type t = typeof(T);
            IntPtr rawClass = AndroidJNI.FindClass(type);
            IntPtr method = AndroidJNI.GetStaticMethodID(rawClass, name, sig);
            jvalue[] jArgs = ConstructArgArray(args);

            try 
            {
                if (t == typeof(bool))
                {
                    return (T)(object)AndroidJNI.CallStaticBooleanMethod(
                        rawClass, method, jArgs);
                }
                else if (t == typeof(string))
                {
                    return (T)(object)AndroidJNI.CallStaticStringMethod(
                        rawClass, method, jArgs);
                }
                else if (t == typeof(int))
                {
                    return (T)(object)AndroidJNI.CallStaticIntMethod(
                        rawClass, method, jArgs);
                }
                else if (t == typeof(float))
                {
                    return (T)(object)AndroidJNI.CallStaticFloatMethod(
                        rawClass, method, jArgs);
                }
                else if (t == typeof(double))
                {
                    return (T)(object)AndroidJNI.CallStaticDoubleMethod(
                        rawClass, method, jArgs);
                }
                else if (t == typeof(byte))
                {
                    return (T)(object)AndroidJNI.CallStaticByteMethod(
                        rawClass, method, jArgs);
                }
                else if (t == typeof(char))
                {
                    return (T)(object)AndroidJNI.CallStaticCharMethod(
                        rawClass, method, jArgs);
                }
                else if (t == typeof(long))
                {
                    return (T)(object)AndroidJNI.CallStaticLongMethod(
                        rawClass, method, jArgs);
                }
                else if (t == typeof(short))
                {
                    return (T)(object)AndroidJNI.CallStaticShortMethod(
                        rawClass, method, jArgs);
                }
                else if (t == typeof(IntPtr))
                {
                    return (T)(object)AndroidJNI.CallStaticObjectMethod(
                        rawClass, method, jArgs);
                }
                else
                {
                    return StaticInvokeObjectCall<T>(type, name, sig, args);
                }
            }
            finally 
            {
                AndroidJNIHelper.DeleteJNIArgArray(args, jArgs);
            }           
        }

        /// <summary>
        /// Invokes a method that returns an object.
        /// </summary>
        /// <returns>The object call.</returns>
        /// <param name="name">Name.</param>
        /// <param name="sig">Sig.</param>
        /// <param name="theArgs">The arguments.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T InvokeObjectCall<T>(string name, string sig,
            params object[] theArgs)
        {
            IntPtr methodId = AndroidJNI.GetMethodID(RawClass, name, sig);

            jvalue[] jArgs = ConstructArgArray(theArgs);

            try
            { 
                IntPtr val = AndroidJNI.CallObjectMethod(raw, methodId, jArgs);

                if (val.Equals(IntPtr.Zero))
                {
                    return default(T);
                }

                ConstructorInfo ctor = typeof(T).GetConstructor(new Type[] { val.GetType() });
                if (ctor != null)
                {
                    return (T)ctor.Invoke(new object[] { val });
                }

                Type t = typeof(T);
                return (T)Marshal.PtrToStructure(val, t);
            }
            finally 
            {
                AndroidJNIHelper.DeleteJNIArgArray(theArgs, jArgs);
            }
        }
    }
}
#endif
