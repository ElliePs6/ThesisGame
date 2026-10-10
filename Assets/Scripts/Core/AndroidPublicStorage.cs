using System.Text;
using UnityEngine;

/// <summary>
/// Αποθηκεύει ένα text αρχείο (π.χ. CSV) στον δημόσιο φάκελο Downloads της
/// συσκευής (Android/Quest), μέσα σε έναν υποφάκελο "Download/AdaptiveAIPrototype".
///
/// Ο λόγος που χρειάζεται αυτό: το Application.persistentDataPath βρίσκεται
/// μέσα στο Android/data/<package>/files/, το οποίο ΔΕΝ είναι προσβάσιμο από
/// το βασικό Files app λόγω scoped storage περιορισμών του Android (API 29+).
/// Το public Downloads directory, αντίθετα, είναι πάντα ορατό στο Files app,
/// χωρίς adb, χωρίς PC, χωρίς ειδικά permissions.
///
/// Χρησιμοποιεί το MediaStore API μέσω AndroidJavaObject -- δεν χρειάζεται
/// κανένα native plugin.
/// </summary>
public static class AndroidPublicStorage
{
    /// <summary>
    /// Γράφει το περιεχόμενο σαν αρχείο μέσα στο Downloads/AdaptiveAIPrototype.
    /// Επιστρέφει true αν πέτυχε. Σε Editor ή μη-Android πλατφόρμες δεν κάνει
    /// τίποτα και επιστρέφει false (ώστε ο caller να ξέρει να μην το θεωρήσει
    /// ως το "κύριο" export path).
    /// </summary>
    public static bool SaveToDownloads(string fileName, string content, string subFolder = "AdaptiveAIPrototype")
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver"))
            using (AndroidJavaObject contentValues = new AndroidJavaObject("android.content.ContentValues"))
            {
                contentValues.Call("put", "_display_name", fileName);
                contentValues.Call("put", "mime_type", "text/csv");
                contentValues.Call("put", "relative_path", $"Download/{subFolder}");

                using (AndroidJavaClass mediaStoreDownloads = new AndroidJavaClass("android.provider.MediaStore$Downloads"))
                using (AndroidJavaObject collectionUri = mediaStoreDownloads.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI"))
                {
                    AndroidJavaObject itemUri = contentResolver.Call<AndroidJavaObject>("insert", collectionUri, contentValues);

                    if (itemUri == null)
                    {
                        Debug.LogWarning("AndroidPublicStorage: insert() returned null -- δεν έγινε δημιουργία εγγραφής στο MediaStore.");
                        return false;
                    }

                    using (AndroidJavaObject outputStream = contentResolver.Call<AndroidJavaObject>("openOutputStream", itemUri))
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(content);
                        outputStream.Call("write", bytes);
                        outputStream.Call("flush");
                        outputStream.Call("close");
                    }

                    Debug.Log($"AndroidPublicStorage: Αποθηκεύτηκε στο Downloads/{subFolder}/{fileName}");
                    return true;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"AndroidPublicStorage: Απέτυχε η αποθήκευση στο Downloads ({fileName}): {e.Message}");
            return false;
        }
#else
        // Editor / PC / άλλη πλατφόρμα -- δεν εφαρμόζεται, το persistentDataPath αρκεί.
        return false;
#endif
    }
}