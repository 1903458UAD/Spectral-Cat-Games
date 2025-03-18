using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperCheats : MonoBehaviour
{
	#region Variables

	//Boolean to control developer cheat availability
	public bool cheatsEnabled;

	//Keycodes
	private KeyCode addCashKey;
	private KeyCode fillOrderKey;
	private KeyCode togglePowerKey;

	//Script references
	private CustomerScript CustomerScript;
	[SerializeField] private PowerCutScript PowerCutScript;

	//Object references
	[SerializeField] private GameObject lights;
	private GameObject customer;

	#endregion

	private void Start()
	{
		addCashKey = KeyCode.Alpha9;
		fillOrderKey = KeyCode.Alpha8;
		togglePowerKey = KeyCode.Alpha7;
	}

	private void addCash()
	{
		GameManager.Instance.UpdateIncome(1000);
	}

	private void fillOrder()
	{
		customer = GameObject.Find("Customer(Clone)");
		CustomerScript = customer.GetComponent<CustomerScript>();
		CustomerScript.Pay();
	}

	private void togglePower()
	{
		if (lights.activeSelf)
		{
			PowerCutScript.tripPower();
		}
		else
		{
			PowerCutScript.fixPower();
		}	
	}

	private void Update()
	{
		if(cheatsEnabled)
		{
			if (Input.GetKeyDown(addCashKey))
			{
				addCash();
			}

			if (Input.GetKeyDown(fillOrderKey))
			{
				fillOrder();
			}

			if (Input.GetKeyDown(togglePowerKey))
			{
				togglePower();
			}
		}
	}
}
