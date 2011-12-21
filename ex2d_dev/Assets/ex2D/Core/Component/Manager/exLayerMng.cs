// ======================================================================================
// File         : exLayerMng.cs
// Author       : Wu Jie 
// Last Change  : 11/06/2011 | 17:23:35 PM | Sunday,November
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
/// 
/// A component to manage draw order
/// 
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
[AddComponentMenu("ex2D Helper/Layer Manager")]
public class exLayerMng : exLayer {

    ///////////////////////////////////////////////////////////////////////////////
    // static
    ///////////////////////////////////////////////////////////////////////////////

    public static int CompareByIndentLevel ( exLayer _a, exLayer _b ) {
        return _a.indentLevel - _b.indentLevel;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    List<exLayer> dirtyLayers = new List<exLayer>();
    float totalDepth = 0.0f;
    bool updateAll = false;

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void Awake () {
        base.Awake();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnPreRender () {
        if ( updateAll ) {
            updateAll = false;

            //
            List<exLayer> layerList = new List<exLayer>();
            List<exLayer> specialLayerList = new List<exLayer>();
            totalDepth = camera.farClipPlane - camera.nearClipPlane;
            totalDepth -= 0.2f; // we leave 1.0 for both near and far clip
            float startFrom = transform.position.z + camera.nearClipPlane + 0.1f;

            int totalLayerCount = 0;
            foreach ( exLayer childLayer in children ) {
                totalLayerCount += AddLayerRecursively ( childLayer, true, ref layerList );
            }

            //
            float unitLayer = totalDepth/totalLayerCount;

            // normal layer depth calcualte
            int specialIndentLevel = 99999;
            float curDepth = startFrom + unitLayer; // skip layerMng
            for ( int i = 0; i < layerList.Count; ++i ) {
                exLayer layer = layerList[i];
                layer.depth = curDepth;

                if ( specialIndentLevel >= layer.indentLevel )
                    specialIndentLevel = 99999;

                if ( specialIndentLevel < layer.indentLevel && layer.type == exLayer.Type.Normal ) {
                    curDepth += unitLayer; 
                }
                else {
                    specialIndentLevel = layer.indentLevel;
                    if ( layer.type == exLayer.Type.Dynamic )
                        curDepth += layer.range; 
                    specialLayerList.Add (layer);
                }
            }

            // special layer depth calculate 
            for ( int i = 0; i < specialLayerList.Count; ++i ) {
                if ( specialLayerList[i].type == exLayer.Type.Dynamic )
                    CalculateDepthForDynamicLayer ( specialLayerList[i], false );
                else if ( specialLayerList[i].type == exLayer.Type.Abstract )
                    CalculateDepthForAbstractLayer ( specialLayerList[i], false );
            }

            // assignment
            for ( int i = 0; i < layerList.Count; ++i ) {
                exLayer layer = layerList[i];
                if ( layer.isDirty == false )
                    continue;

                RecursivelyUpdateLayer ( layer.transform.root );
            }
        }
        else {
            // re-update special layers
            for ( int i = 0; i < dirtyLayers.Count; ++i ) {
                exLayer layer = dirtyLayers[i];
                if ( layer.type == exLayer.Type.Dynamic )
                    CalculateDepthForDynamicLayer ( layer, true );
                else if ( layer.type == exLayer.Type.Abstract )
                    CalculateDepthForAbstractLayer ( layer, true );
            }
        }

        dirtyLayers.Clear();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void RecursivelyUpdateLayer ( Transform _trans ) {
        //
        exLayer layer = _trans.GetComponent<exLayer>(); 
        if ( layer != null && layer.isDirty == true ) {
            layer.isDirty = false;
            _trans.position = new Vector3 ( _trans.position.x, _trans.position.y, layer.depth );
        }

        //
        foreach ( Transform child in _trans ) {
            RecursivelyUpdateLayer ( child );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    int AddLayerRecursively ( exLayer _curLayer, 
                              bool _doCount, 
                              ref List<exLayer> _layerList ) {
        int count = 1;
        bool doCount = _doCount;

        //
        _curLayer.isDirty = true;
        _layerList.Add ( _curLayer );

        //
        if ( _curLayer.type != exLayer.Type.Normal ) {
            doCount = false;
        }

        //
        foreach ( exLayer childLayer in _curLayer.children ) {
            count += AddLayerRecursively ( childLayer, doCount, ref _layerList );
        }

        return count;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void SetLayerDepthRecursively ( exLayer _curLayer, float _depth ) {
        _curLayer.depth = _depth;

        foreach ( exLayer childLayer in _curLayer.children ) {
            SetLayerDepthRecursively ( childLayer, _depth );
        }
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void CalculateDepthForDynamicLayer ( exLayer _curLayer, bool _doAssign ) {
        List<exLayer> layerList = new List<exLayer>();
        int totalLayerCount = AddLayerRecursively ( _curLayer, true, ref layerList );
        float unitLayer = _curLayer.range/totalLayerCount;
        float curDepth = _curLayer.depth;

        for ( int i = 0; i < layerList.Count; ++i ) {
            exLayer layer = layerList[i];
            layer.depth = curDepth;
            if ( layer.type == exLayer.Type.Normal ) {
                curDepth += unitLayer; 
            }
        }

        // assignment
        if ( _doAssign ) {
            for ( int i = 0; i < layerList.Count; ++i ) {
                exLayer layer = layerList[i];
                if ( layer.isDirty == false )
                    continue;

                RecursivelyUpdateLayer ( layer.transform.root );
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void CalculateDepthForAbstractLayer ( exLayer _curLayer, bool _doAssign ) {
        List<exLayer> layerList = new List<exLayer>();
        AddLayerRecursively ( _curLayer, true, ref layerList );
        float curDepth = _curLayer.depth;

        for ( int i = 0; i < layerList.Count; ++i ) {
            exLayer layer = layerList[i];
            layer.depth = curDepth;
        }

        // assignment
        if ( _doAssign ) {
            for ( int i = 0; i < layerList.Count; ++i ) {
                exLayer layer = layerList[i];
                if ( layer.isDirty == false )
                    continue;

                RecursivelyUpdateLayer ( layer.transform.root );
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddDirtyLayer ( exLayer _layer ) {
        if ( _layer.type == exLayer.Type.Normal ) {
            updateAll = true;
        }
        else {
            dirtyLayers.Add (_layer);
            _layer.isDirty = true;
        }
    }
}
