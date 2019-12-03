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

import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.common.api.Result;
import com.google.android.gms.common.api.Status;

/**
 * Class for returning the tokens to Unity.  These are combined into 1 call
 * to make it easy to implement.
 */
public class TokenResult implements Result {
    private Status status;
    private GoogleSignInAccount account;

    public TokenResult() {

    }

    /**
     * Returns a string containing a concise, human-readable description of this
     * object. Subclasses are encouraged to override this method and provide an
     * implementation that takes into account the object's type and data. The
     * default implementation is equivalent to the following expression:
     * <pre>
     *   getClass().getName() + '@' + Integer.toHexString(hashCode())</pre>
     * <p>See <a href="{@docRoot}reference/java/lang/Object.html#writing_toString">Writing a useful
     * {@code toString} method</a>
     * if you intend implementing your own {@code toString} method.
     *
     * @return a printable representation of this object.
     */
    @Override
    public String toString() {
        return "Status: " + status + " email: " + getEmail() + " id:" +
                getIdToken() + " access: " + getAuthCode();
    }

    @Override
    public Status getStatus() {
        return status;
    }

    public int getStatusCode() { return status.getStatusCode();}

    public GoogleSignInAccount getAccount() {
        return account;
    }

    public String getAuthCode() {
        String result = account == null ? "" : account.getServerAuthCode();
        return result == null ? "" : result;
    }

    public String getIdToken() {
        String result = account == null ? "" : account.getIdToken();
        return result == null ? "" : result;
    }

    public String getEmail() {
        String result = account == null ? "" : account.getEmail();
        return result == null ? "" : result;
    }

    public void setStatus(int status) {
        this.status = new Status(status);
    }

    public void setAccount(GoogleSignInAccount account) {
        this.account = account;
    }
}
