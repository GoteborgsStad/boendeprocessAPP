using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Text))]
	public class VersionText : MonoBehaviour 
	{
		void Awake()
		{
			GetComponent<Text>().text = Application.version + " - " + BuildNumberHolder.BuildNumber + " - " + EnvironmentConfiguration.TargetSite;
		}
	}
}