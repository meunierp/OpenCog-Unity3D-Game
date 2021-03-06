
/// Unity3D OpenCog World Embodiment Program
/// Copyright (C) 2013  Novamente			
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Affero General Public License for more details.
///
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using OpenCog.Attributes;
using OpenCog.Extensions;
using ProtoBuf;
using UnityEngine;

namespace OpenCog
{

namespace Actions
{

/// <summary>
/// The OpenCog OCIdleAction.
/// </summary>
#region Class Attributes

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[OCExposePropertyFields]
[Serializable]
[ExecuteInEditMode]
#endregion
public class OCRunForwardAction : OCBasicAnimationAction
{

	//---------------------------------------------------------------------------

	#region Private Member Data

	//---------------------------------------------------------------------------


	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Accessors and Mutators

	//---------------------------------------------------------------------------
			
	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------	

	#region Constructors

	//---------------------------------------------------------------------------

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Public Member Functions

	//---------------------------------------------------------------------------

	/// <summary>
	/// Called when the script instance is being loaded.
	/// </summary>


	public override void Initialize()
	{
		HasAnimation = true;
		IsTranslation = true;
		AnimationEffect = new OCAnimationEffect();//ScriptableObject.CreateInstance<OCAnimation>();

		AnimationEffect.Initialize(gameObject, animation["run"]);
		AnimationEffect.Position = gameObject.transform.position;
		AnimationEffect.State.wrapMode = WrapMode.Once;
		AnimationEffect.State.layer = 0;
		AnimationEffect.State.speed = 1.0f;

		AnimationEffect.MoveByZ = 3.0f;

		AnimationEffect.OnStart = "RunForwardStart";
		AnimationEffect.OnEnd = "RunForwardEnd";


		DontDestroyOnLoad(this);
	}

//	public void Update()
//	{
//		if(IsExecuting())
//		{
//			gameObject.transform.Translate(0, 0, Animation.MoveByZ / Animation.Time * Time.deltaTime);
//		}
//	}

	public void RunForwardStart()
	{
		BasicAnimationStart();
	}

	public void RunForwardEnd()
	{
		BasicAnimationEnd();
	}

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Private Member Functions

	//---------------------------------------------------------------------------
			
	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Member Classes

	//---------------------------------------------------------------------------		

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

}// class OCIdleAction

}// namespace Actions

}// namespace OpenCog




