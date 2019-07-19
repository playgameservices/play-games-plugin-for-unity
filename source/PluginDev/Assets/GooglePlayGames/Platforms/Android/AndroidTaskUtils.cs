#if UNITY_ANDROID

namespace GooglePlayGames.Android
{
    using UnityEngine;
    using System;

    class AndroidTaskUtils
    {
        private AndroidTaskUtils()
        {
        }

        public static void AddOnSuccessListener<T>(AndroidJavaObject task, Action<T> callback)
        {
            using (task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<T>(callback))) ;
        }

        public static void AddOnFailureListener(AndroidJavaObject task, Action<AndroidJavaObject> callback)
        {
            using (task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(callback))) ;
        }

        public static void AddOnCompleteListener<T>(AndroidJavaObject task, Action<T> callback)
        {
            using (task.Call<AndroidJavaObject>("addOnCompleteListener", new TaskOnCompleteProxy<T>(callback))) ;
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
            private Action<AndroidJavaObject> mCallback2;

            public TaskOnSuccessProxy(Action<T> callback)
                : base("com/google/android/gms/tasks/OnSuccessListener")
            {
                mCallback = callback;
            }

            public void onSuccess(T result)
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

        private class TaskOnFailedProxy : AndroidJavaProxy
        {
            private Action<AndroidJavaObject> mCallback;

            public TaskOnFailedProxy(Action<AndroidJavaObject> callback)
                : base("com/google/android/gms/tasks/OnFailureListener")
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
    }
}
#endif