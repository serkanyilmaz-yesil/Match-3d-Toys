#if TTP_PRIVACY_SETTINGS
using UnityEngine;
using Tabtale.TTPlugins;

public class PrivacySettingsPopUp : MonoBehaviour {

	public void OnClickPA()
	{
        TTPPrivacySettings.CustomConsentSetConsent (TTPPrivacySettings.ConsentType.PA);
		Destroy (this.gameObject);
	}

	public void OnClickNPA()
	{
		TTPPrivacySettings.CustomConsentSetConsent(TTPPrivacySettings.ConsentType.NPA);
		Destroy (this.gameObject);
	}

	public void OnClickForgetMe()
	{
		TTPPrivacySettings.ForgetUser ();
		Destroy (this.gameObject);
	}
}
#endif