/*
 * Copyright (C) Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.google.games.bridge;

import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.common.api.ResultCallback;

import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;


/**
 * Pending result class for TokenResult.  This allows the
 * pending result to be returned to the caller, and then updated when available, simplifying the
 * handling of callbacks.
 */
public class TokenPendingResult extends PendingResult<TokenResult> {

    private CountDownLatch latch = new CountDownLatch(1);
    /* package local */ TokenResult result;
    private ResultCallback<? super TokenResult> resultCallback;

    public TokenPendingResult()
    {
        result = new TokenResult();
    }
    @Override
    public TokenResult await() {

        try {
            latch.await();
        } catch (InterruptedException e) {
            setResult(null, null, null, CommonStatusCodes.INTERRUPTED);
        }

        return getResult();
    }

    @Override
    public TokenResult await(long l, TimeUnit timeUnit) {
        try {
            if (!latch.await(l, timeUnit)) {
                setResult(null, null, null, CommonStatusCodes.TIMEOUT);
            }
        } catch (InterruptedException e) {
            setResult(null, null, null, CommonStatusCodes.INTERRUPTED);
        }
        return getResult();
    }

    @Override
    public void cancel() {
        setResult(null, null, null, CommonStatusCodes.CANCELED);
        latch.countDown();
    }

    @Deprecated
    void setToken(String accessToken, String idToken, String email, int resultCode) {
        setResult(accessToken, idToken, email, resultCode);
        latch.countDown();
        if (getCallback() != null) {
            getCallback().onResult(getResult());
        }
    }

    @Override
    public boolean isCanceled() {
        return getResult() != null && getResult().getStatus().isCanceled();
    }

    @Override
    public void setResultCallback(ResultCallback<? super TokenResult> resultCallback) {

        // Handle adding the callback when the latch has already counted down.  This
        // can happen if there is an error right away.
        if (latch.getCount() == 0) {
            resultCallback.onResult(getResult());
        } else {
            setCallback(resultCallback);
        }
    }

    @Override
    public void setResultCallback(ResultCallback<? super TokenResult> resultCallback, long l,
                                  TimeUnit timeUnit) {
        try {
            if (!latch.await(l, timeUnit)) {
                setResult(null, null, null, CommonStatusCodes.TIMEOUT);
            }
        } catch (InterruptedException e) {
            setResult(null, null, null, CommonStatusCodes.INTERRUPTED);
        }

        resultCallback.onResult(getResult());

    }

    private synchronized void setCallback(ResultCallback<? super TokenResult> callback) {
        this.resultCallback = callback;
    }

    private synchronized ResultCallback<? super TokenResult> getCallback() {
        return this.resultCallback;
    }

    /**
     * Set the result.  If any of the values are null, and a previous non-null value was set,
     * the non-null value is retained.
     *
     * @param accessToken - the access token
     * @param idToken     - the id token
     * @param email       - user's email  (aka accountName).
     * @param resultCode  - the result code.
     */
    private synchronized void setResult(String accessToken, String idToken, String email, int resultCode) {
        String atok = (result != null && accessToken == null) ? result.getAccessToken() : accessToken;
        String itok = (result != null && idToken == null) ? result.getIdToken() : idToken;
        String em = (result != null && email == null) ? result.getEmail() : email;

        result = new TokenResult(atok, itok, em, resultCode);
    }

    private synchronized TokenResult getResult() {
        return result;
    }


    /**
     * Sets the result status and releases the latch and/or calls the callback.
     * @param status - the result status.
     */
    public void setStatus(int status) {
        result.setStatus(status);
        latch.countDown();
        if (getCallback() != null) {
            getCallback().onResult(getResult());
        }

    }

    public void setEmail(String email) {
        result.setEmail(email);
    }

    public void setAccessToken(String accessToken) {
        result.setAccessToken(accessToken);
    }

    public void setIdToken(String idToken) {
       result.setIdToken(idToken);
    }
}
