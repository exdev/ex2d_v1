// ======================================================================================
// File         : exVolumeTo.cs
// Author       : Wu Jie 
// Last Change  : 06/23/2011 | 12:18:16 PM | Thursday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D Helper/EffectTo/Volume To")]
public class exVolumeTo : exEffectToFloat {
	
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	void Update () {
        audio.volume = Step();
	}

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override void Play () {
        PlayFrom ( audio.volume );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override void Stop () {
        if ( enabled == true )
            audio.volume = StopAt();
    }
}
