using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AnimatorConfigurator
{
	[SerializeField] public Animator animator = null;
	[SerializeField] public List<AnimatorParameter> animatorParams = new List<AnimatorParameter>();
}

