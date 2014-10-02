/*
 * Copyright (C) 2014 Google Inc.
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

#ifndef GooglePlayGamesUnityPlugin_GPGSPTypedefs_h
#define GooglePlayGamesUnityPlugin_GPGSPTypedefs_h

// Because the world needs one more boolean type
// (and because we have to guarantee that this boolean type is 4 bytes long, as expected
// by C#)
typedef int32_t GPGSBOOL;  // 4 bytes
#define GPGSTRUE 1
#define GPGSFALSE 0

// Callback type for success/failure
typedef void (*GPGSSuccessCallback)(GPGSBOOL success, int32_t userData);

// Cloud save callback signatures:
#define kCloudSaveSize 512*1024
typedef unsigned char* GPGSBUF;
typedef const unsigned char* GPGSCBUF;
typedef void (*GPGSUpdateStateCallback)(GPGSBOOL success, int32_t slot);
typedef int (*GPGSStateConflictCallback)(int32_t slot,
GPGSCBUF localBuf, int32_t localDataSize,
GPGSCBUF serverBuf, int32_t serverDataSize,
GPGSBUF resolvedBuf, int32_t resolvedBufCapacity);
typedef void (*GPGSLoadStateCallback)(GPGSBOOL success, int32_t slot, GPGSCBUF buf, int32_t dataSize);

// Invitation received callback

typedef void (*GPGSPushNotificationCallback)(GPGSBOOL isRealTime,
const char * invitationId, const char * inviterId,
const char * inviterName, int32_t variant);

typedef void (*GPGRealTimeRoomStatusChangedCallback)(int32_t status);
typedef void (*GPGRealTimeParticipantListChangedCallback)(const char *returnme);
typedef void (*GPGRealTimeDataReceivedCallback)(const char *participantId, GPGSCBUF data, int32_t dataLen, GPGSBOOL isReliable);
typedef void (*GPGRealTimeRoomErrorCallback)(const char *localizedError, int32_t errorCode);

typedef void (*GPGTurnBasedMatchCreateCallback)(const char *matchAsJson, GPGSBOOL isError, int32_t callbackId);
typedef void (*GPGTurnBasedSuccessCallback)(GPGSBOOL isError, int32_t errorCode, int32_t callbackId);
typedef void (*GPGSTurnBasedMatchYourTurnCallback)(GPGSBOOL gameOver, const char *matchAsJson);
#endif
