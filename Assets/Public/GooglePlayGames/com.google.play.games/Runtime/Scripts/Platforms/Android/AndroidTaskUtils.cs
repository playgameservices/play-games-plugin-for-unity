#if UNITY_ANDROID
#pragma warning disable 0642 // Possible mistaken empty statement

namespace GooglePlayGames.Android
{
    using UnityEngine;
    using System;

    static class AndroidTaskUtils
    {
        /** <returns> self </returns> */
        public static AndroidJavaObject AddOnCanceledListener(this AndroidJavaObject task, Action callback)
        {
            using (task.Call<AndroidJavaObject>("addOnCanceledListener",new TaskOnCanceledProxy(callback))) ;
            return task;
        }

        /** <returns> self </returns> */
        public static AndroidJavaObject AddOnSuccessListener<T>(this AndroidJavaObject task, Action<T> callback) => task.AddOnSuccessListener(true,callback);

        /** <returns> self </returns> */
        public static AndroidJavaObject AddOnSuccessListener<T>(this AndroidJavaObject task, bool disposeResult, Action<T> callback)
        {
            using (task.Call<AndroidJavaObject>("addOnSuccessListener",new TaskOnSuccessProxy<T>(callback, disposeResult))) ;
            return task;
        }

        /** <returns> self </returns> */
        public static AndroidJavaObject AddOnFailureListener(this AndroidJavaObject task, Action<AndroidJavaObject> callback)
        {
            using (task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(callback))) ;
            return task;
        }

        /** <returns> self </returns> */
        public static AndroidJavaObject AddOnCompleteListener<T>(this AndroidJavaObject task, Action<T> callback)
        {
            using (task.Call<AndroidJavaObject>("addOnCompleteListener", new TaskOnCompleteProxy<T>(callback))) ;
            return task;
        }

        private class TaskOnCompleteProxy<T> : AndroidJavaProxy
        {
            private Action<T> mCallback;

            public TaskOnCompleteProxy(Action<T> callback)
                : base("com/google/android/gms/tasks/OnCompleteListener")
            {
                mCallback = callback;
            }

            public void onComplete(T result)
            {
                if (result is IDisposable)
                {
                    using ((IDisposable) result)
                    {
                        mCallback(result);
                    }
                }
                else
                {
                    mCallback(result);
                }
            }
        }

        private class TaskOnSuccessProxy<T> : AndroidJavaProxy
        {
            private Action<T> mCallback;
            private bool mDisposeResult;

            public TaskOnSuccessProxy(Action<T> callback, bool disposeResult) : base("com/google/android/gms/tasks/OnSuccessListener")
            {
                mCallback = callback;
                mDisposeResult = disposeResult;
            }

            public void onSuccess(T result)
            {
                if (result is IDisposable && mDisposeResult)
                {
                    using ((IDisposable) result)
                    {
                        mCallback(result);
                    }
                }
                else
                {
                    mCallback(result);
                }
            }
        }

        private class TaskOnFailedProxy : AndroidJavaProxy
        {
            private Action<AndroidJavaObject> mCallback;

            public TaskOnFailedProxy(Action<AndroidJavaObject> callback) : base("com/google/android/gms/tasks/OnFailureListener")
            {
                mCallback = callback;
            }

            public void onFailure(AndroidJavaObject exception)
            {
                using (exception)
                {
                    mCallback(exception);
                }
            }
        }

        private class TaskOnCanceledProxy : AndroidJavaProxy
        {
            private Action mCallback;

            public TaskOnCanceledProxy(Action callback) : base("com/google/android/gms/tasks/OnCanceledListener")
            {
                mCallback = callback;
            }

            public void onCanceled()
            {
                mCallback();
            }
        }
    }
}
#endif
