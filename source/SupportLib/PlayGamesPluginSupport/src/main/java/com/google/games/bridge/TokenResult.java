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

import com.google.android.gms.common.api.Result;
import com.google.android.gms.common.api.Status;

/**
 * Class for returning the tokens to Unity.  These are combined into 1 call
 * to make it easy to implement.
 */
public class TokenResult implements Result {
    private Status status;
    private String accessToken;
    private String idToken;
    private String email;

    public TokenResult() {

    }

    TokenResult(String accessToken, String idToken, String email, int resultCode) {
        status = new Status(resultCode);
        this.accessToken = accessToken;
        this.idToken = idToken;
        this.email = email;
    }

    @Override
    public Status getStatus() {
        return status;
    }

    public String getAccessToken() {
        return accessToken;
    }

    public String getIdToken() {
        return idToken;
    }

    public String getEmail() {
        return email;
    }

    public void setStatus(int status) {
        this.status = new Status(status);
    }

    public void setEmail(String email) {
        this.email = email;
    }

    public void setAccessToken(String accessToken) {
        this.accessToken = accessToken;
    }

    public void setIdToken(String idToken) {
        this.idToken = idToken;
    }
}