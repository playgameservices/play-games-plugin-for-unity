#if UNITY_ANDROID

namespace GooglePlayGames.Android
{
    using UnityEngine;
    using System;

    class TaskOnCompleteProxy<T> : AndroidJavaProxy
    {
        private Action<T> mCallback;

        public TaskOnCompleteProxy(Action<T> callback) 
        : base("com/google/android/gms/tasks/OnCompleteListener")
        {
            mCallback = callback;
        }

        public void onComplete(T result)
        {
            mCallback(result);
        }
    }

    class TaskOnSuccessProxy<T> : AndroidJavaProxy
    {
        private Action<T> mCallback;

        public TaskOnSuccessProxy(Action<T> callback) 
        : base("com/google/android/gms/tasks/OnSuccessListener")
        {
            mCallback = callback;
        }

        public void onSuccess(T result)
        {
            mCallback(result);
        }
    }

    class TaskOnFailedProxy : AndroidJavaProxy
    {
        private Action<AndroidJavaObject> mCallback;

        public TaskOnFailedProxy(Action<AndroidJavaObject> callback) 
        : base("com/google/android/gms/tasks/OnFailureListener")
        {
            mCallback = callback;
        }

        public void onFailure(AndroidJavaObject exception)
        {
            mCallback(exception);
        }
    }
}
#endif
