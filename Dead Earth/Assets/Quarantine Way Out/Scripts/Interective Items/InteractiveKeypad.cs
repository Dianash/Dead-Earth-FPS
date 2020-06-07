using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveKeypad : InteractiveItem
{
	[SerializeField] protected Transform elevator = null;
	[SerializeField] protected AudioCollection collection = null;
	[SerializeField] protected int bank = 0;
	[SerializeField] protected float activationDelay = 0.0f;

	bool isActivated = false;
}
