package com.google.example.games.basegameutils;

import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.content.pm.Signature;
import android.content.res.Resources;
import android.util.Log;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.games.GamesActivityResultCodes;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

/**
 * Created by btco on 2/10/14.
 */
class GameHelperUtils {
    public static final int R_UNKNOWN_ERROR = 0;
    public static final int R_SIGN_IN_FAILED = 1;
    public static final int R_APP_MISCONFIGURED = 2;
    public static final int R_LICENSE_FAILED = 3;

    private final static String[] FALLBACK_STRINGS = {
            "*Unknown error.",
            "*Failed to sign in. Please check your network connection and try again.",
            "*The application is incorrectly configured. Check that the package name and signing certificate match the client ID created in Developer Console. Also, if the application is not yet published, check that the account you are trying to sign in with is listed as a tester account. See logs for more information.",
            "*License check failed."
    };

    private final static int[] RES_IDS = {
            R.string.gamehelper_unknown_error, R.string.gamehelper_sign_in_failed,
            R.string.gamehelper_app_misconfigured, R.string.gamehelper_license_failed
    };

    static String activityResponseCodeToString(int respCode) {
        switch (respCode) {
            case Activity.RESULT_OK:
                return "RESULT_OK";
            case Activity.RESULT_CANCELED:
                return "RESULT_CANCELED";
            case GamesActivityResultCodes.RESULT_APP_MISCONFIGURED:
                return "RESULT_APP_MISCONFIGURED";
            case GamesActivityResultCodes.RESULT_LEFT_ROOM:
                return "RESULT_LEFT_ROOM";
            case GamesActivityResultCodes.RESULT_LICENSE_FAILED:
                return "RESULT_LICENSE_FAILED";
            case GamesActivityResultCodes.RESULT_RECONNECT_REQUIRED:
                return "RESULT_RECONNECT_REQUIRED";
            case GamesActivityResultCodes.RESULT_SIGN_IN_FAILED:
                return "SIGN_IN_FAILED";
            default:
                return String.valueOf(respCode);
        }
    }

    static String errorCodeToString(int errorCode) {
        switch (errorCode) {
            case ConnectionResult.DEVELOPER_ERROR:
                return "DEVELOPER_ERROR(" + errorCode + ")";
            case ConnectionResult.INTERNAL_ERROR:
                return "INTERNAL_ERROR(" + errorCode + ")";
            case ConnectionResult.INVALID_ACCOUNT:
                return "INVALID_ACCOUNT(" + errorCode + ")";
            case ConnectionResult.LICENSE_CHECK_FAILED:
                return "LICENSE_CHECK_FAILED(" + errorCode + ")";
            case ConnectionResult.NETWORK_ERROR:
                return "NETWORK_ERROR(" + errorCode + ")";
            case ConnectionResult.RESOLUTION_REQUIRED:
                return "RESOLUTION_REQUIRED(" + errorCode + ")";
            case ConnectionResult.SERVICE_DISABLED:
                return "SERVICE_DISABLED(" + errorCode + ")";
            case ConnectionResult.SERVICE_INVALID:
                return "SERVICE_INVALID(" + errorCode + ")";
            case ConnectionResult.SERVICE_MISSING:
                return "SERVICE_MISSING(" + errorCode + ")";
            case ConnectionResult.SERVICE_VERSION_UPDATE_REQUIRED:
                return "SERVICE_VERSION_UPDATE_REQUIRED(" + errorCode + ")";
            case ConnectionResult.SIGN_IN_REQUIRED:
                return "SIGN_IN_REQUIRED(" + errorCode + ")";
            case ConnectionResult.SUCCESS:
                return "SUCCESS(" + errorCode + ")";
            default:
                return "Unknown error code " + errorCode;
        }
    }

    static void printMisconfiguredDebugInfo(Context ctx) {
        Log.w("GameHelper", "****");
        Log.w("GameHelper", "****");
        Log.w("GameHelper", "**** APP NOT CORRECTLY CONFIGURED TO USE GOOGLE PLAY GAME SERVICES");
        Log.w("GameHelper", "**** This is usually caused by one of these reasons:");
        Log.w("GameHelper", "**** (1) Your package name and certificate fingerprint do not match");
        Log.w("GameHelper", "****     the client ID you registered in Developer Console.");
        Log.w("GameHelper", "**** (2) Your App ID was incorrectly entered.");
        Log.w("GameHelper", "**** (3) Your game settings have not been published and you are ");
        Log.w("GameHelper", "****     trying to log in with an account that is not listed as");
        Log.w("GameHelper", "****     a test account.");
        Log.w("GameHelper", "****");
        if (ctx == null) {
            Log.w("GameHelper", "*** (no Context, so can't print more debug info)");
            return;
        }

        Log.w("GameHelper", "**** To help you debug, here is the information about this app");
        Log.w("GameHelper", "**** Package name         : " + ctx.getPackageName());
        Log.w("GameHelper", "**** Cert SHA1 fingerprint: " + getSHA1CertFingerprint(ctx));
        Log.w("GameHelper", "**** App ID from          : " + getAppIdFromResource(ctx));
        Log.w("GameHelper", "****");
        Log.w("GameHelper", "**** Check that the above information matches your setup in ");
        Log.w("GameHelper", "**** Developer Console. Also, check that you're logging in with the");
        Log.w("GameHelper", "**** right account (it should be listed in the Testers section if");
        Log.w("GameHelper", "**** your project is not yet published).");
        Log.w("GameHelper", "****");
        Log.w("GameHelper", "**** For more information, refer to the troubleshooting guide:");
        Log.w("GameHelper", "****   http://developers.google.com/games/services/android/troubleshooting");
    }

    static String getAppIdFromResource(Context ctx) {
        try {
            Resources res = ctx.getResources();
            String pkgName = ctx.getPackageName();
            int res_id = res.getIdentifier("app_id", "string", pkgName);
            return res.getString(res_id);
        } catch (Exception ex) {
            ex.printStackTrace();
            return "??? (failed to retrieve APP ID)";
        }
    }

    static String getSHA1CertFingerprint(Context ctx) {
        try {
            Signature[] sigs = ctx.getPackageManager().getPackageInfo(
                    ctx.getPackageName(), PackageManager.GET_SIGNATURES).signatures;
            if (sigs.length == 0) {
                return "ERROR: NO SIGNATURE.";
            } else if (sigs.length > 1) {
                return "ERROR: MULTIPLE SIGNATURES";
            }
            byte[] digest = MessageDigest.getInstance("SHA1").digest(sigs[0].toByteArray());
            StringBuilder hexString = new StringBuilder();
            for (int i = 0; i < digest.length; ++i) {
                if (i > 0) {
                    hexString.append(":");
                }
                byteToString(hexString, digest[i]);
            }
            return hexString.toString();

        } catch (PackageManager.NameNotFoundException ex) {
            ex.printStackTrace();
            return "(ERROR: package not found)";
        } catch (NoSuchAlgorithmException ex) {
            ex.printStackTrace();
            return "(ERROR: SHA1 algorithm not found)";
        }
    }

    static void byteToString(StringBuilder sb, byte b) {
        int unsigned_byte = b < 0 ? b + 256 : b;
        int hi = unsigned_byte / 16;
        int lo = unsigned_byte % 16;
        sb.append("0123456789ABCDEF".substring(hi, hi + 1));
        sb.append("0123456789ABCDEF".substring(lo, lo + 1));
    }

    static String getString(Context ctx, int whichString) {
        whichString = whichString >= 0 && whichString < RES_IDS.length ? whichString : 0;
        int resId = RES_IDS[whichString];
        try {
            return ctx.getString(resId);
        } catch (Exception ex) {
            ex.printStackTrace();
            Log.w(GameHelper.TAG, "*** GameHelper could not found resource id #" + resId + ". " +
                "This probably happened because you included it as a stand-alone JAR. " +
                "BaseGameUtils should be compiled as a LIBRARY PROJECT, so that it can access " +
                "its resources. Using a fallback string.");
            return FALLBACK_STRINGS[whichString];
        }
    }
}
