using HarmonyLib;
using UnityEngine;
using iiMenu.Menu;
using iiMenu.Mods;
using static NetworkSystem;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Animations.Rigging;


public class AdvancedGunLib : MonoBehaviour
{
    private static Vector3 CalculateBezierPoint(Vector3 start, Vector3 mid, Vector3 end, float t) => (1f - t) * (1f - t) * start + 2f * (1f - t) * t * mid + t * t * end;

    public static void CurveLineRenderer(LineRenderer lineRenderer, Vector3 start, Vector3 mid, Vector3 end)
    {
        lineRenderer.positionCount = AdvancedGunLib.LineCurve;
        for (int i = 0; i < AdvancedGunLib.LineCurve; i++)
        {
            float num = (float)i / (float)(AdvancedGunLib.LineCurve - 1);
            Vector3 vector = AdvancedGunLib.CalculateBezierPoint(start, mid, end, num);
            lineRenderer.SetPosition(i, vector);
        }
    }

    public static IEnumerator StartCurvyLineRenderer(LineRenderer lineRenderer, Vector3 start, Vector3 mid, Vector3 end)
    {
        for (; ; )
        {
            AdvancedGunLib.CurveLineRenderer(lineRenderer, start, mid, end);
            yield return null;
        }
        yield break;
    }

    public static int LineCurve = 150;
    public static Vector3 lr;
}

[HarmonyPatch(typeof(Main), "RenderGun")]
class RenderGunPatch
{
    public static Vector3 lrr;
    private static Vector3 GunPositionSmoothed = Vector3.zero;

    static bool Prefix(ref (RaycastHit Ray, GameObject NewPointer) __result)
    {
        Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position - (Main.legacyGunDirection ? GorillaTagger.Instance.rightHandTransform.up : Vector3.zero), Main.legacyGunDirection ? -GorillaTagger.Instance.rightHandTransform.up : GorillaTagger.Instance.rightHandTransform.forward, out var Ray, 512f, Main.NoInvisLayerMask());
        if (Main.shouldBePC)
        {
            Ray ray = Main.TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
            Physics.Raycast(ray, out Ray, 512f, Main.NoInvisLayerMask());
        }
        
        Vector3 StartPosition = Main.SwapGunHand ? GorillaTagger.Instance.leftHandTransform.position : GorillaTagger.Instance.rightHandTransform.position;
        Vector3 EndPosition = Main.isCopying ? Main.whoCopy.transform.position : Ray.point;

        if (!Main.isCopying)
        {
            GunPositionSmoothed = Vector3.Lerp(GunPositionSmoothed, EndPosition, Time.deltaTime * 5f);
            EndPosition = GunPositionSmoothed;
        }

        GameObject NewPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        NewPointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
        NewPointer.GetComponent<Renderer>().material.color = (Main.isCopying || (Main.rightTrigger > 0.5f || Mouse.current.leftButton.isPressed)) ? Main.GetBDColor(0f) : Main.GetBRColor(0f);
        NewPointer.transform.localScale = Main.smallGunPointer ? new Vector3(0.1f, 0.1f, 0.1f) : new Vector3(0.2f, 0.2f, 0.2f);
        NewPointer.transform.position = EndPosition;
        
        if (Main.disableGunPointer)
        {
            NewPointer.GetComponent<Renderer>().enabled = false;
        }
        UnityEngine.Object.Destroy(NewPointer.GetComponent<BoxCollider>());
        UnityEngine.Object.Destroy(NewPointer.GetComponent<Rigidbody>());
        UnityEngine.Object.Destroy(NewPointer.GetComponent<Collider>());
        UnityEngine.Object.Destroy(NewPointer, Time.deltaTime);

        if (!Main.disableGunLine)
        {
            AdvancedGunLib.lr = Vector3.Lerp(AdvancedGunLib.lr, (GorillaTagger.Instance.rightHandTransform.position + NewPointer.transform.position) / 2f, Time.deltaTime * 6f);

            GameObject line = new GameObject("Line");
            LineRenderer liner = line.AddComponent<LineRenderer>();
            liner.material.shader = Shader.Find("GUI/Text Shader");
            liner.startColor = Main.GetBGColor(0f);
            liner.endColor = Main.GetBGColor(0.5f);
            liner.startWidth = 0.025f;
            liner.endWidth = 0.025f;
            liner.positionCount = 2;
            liner.useWorldSpace = true;
            liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
            liner.SetPosition(1, Main.isCopying ? Main.whoCopy.transform.position : Ray.point);
            AdvancedGunLib.CurveLineRenderer(liner, GorillaTagger.Instance.rightHandTransform.position, AdvancedGunLib.lr, NewPointer.transform.position);
            UnityEngine.Object.Destroy(line, Time.deltaTime);
        }

        VRRig possibly = Ray.collider.GetComponentInParent<VRRig>();
        if (possibly != null) GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength / 5f, Time.fixedDeltaTime);

        __result = (Ray, NewPointer);
        return false;
    }
}
