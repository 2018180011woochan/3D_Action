using UnityEngine;

public class MutantLeftHandAttackBehaviours : StateMachineBehaviour
{
    public string hitboxPath =
        "Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_LeftShoulder/Character1_LeftArm/Character1_LeftForeArm/Character1_LeftHand/HitBox";

    public int windowStartFrame = 2;
    public int windowEndFrame = 32;

    Transform hitBox;
    AttackHitBox hit;
    float startN, endN;
    bool active;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!hitBox)
        {
            hitBox = TryFindByPathOrLeaf(animator.transform, hitboxPath);
            if (hitBox) hit = hitBox.GetComponent<AttackHitBox>();
        }

        hitBox.gameObject.SetActive(false);
        active = false;

        var infos = animator.GetCurrentAnimatorClipInfo(layerIndex);
        if (infos.Length > 0 && infos[0].clip)
        {
            var clip = infos[0].clip;
            int totalFrames = Mathf.Max(1, Mathf.RoundToInt(clip.length * clip.frameRate));
            startN = Mathf.Clamp01(windowStartFrame / (float)totalFrames);
            endN = Mathf.Clamp01(windowEndFrame / (float)totalFrames);
        }
        else
        {
            float total = stateInfo.length * 60f;
            startN = Mathf.Clamp01(windowStartFrame / total);
            endN = Mathf.Clamp01(windowEndFrame / total);
        }

        hit.ResetSwing();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!hitBox) return;

        float norm = stateInfo.normalizedTime - Mathf.Floor(stateInfo.normalizedTime);
        bool inWindow = norm >= startN && norm <= endN;

        if (inWindow && !active)
        {
            hit.ResetSwing();
            hitBox.gameObject.SetActive(true);
            active = true;
        }
        else if (!inWindow && active)
        {
            hitBox.gameObject.SetActive(false);
            active = false;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hitBox) hitBox.gameObject.SetActive(false);
        active = false;
    }

    static Transform TryFindByPathOrLeaf(Transform root, string path)
    {
        if (root == null || string.IsNullOrEmpty(path)) return null;

        var t = root.Find(path);
        if (t) return t;

        string leaf = path.Contains("/") ? path.Substring(path.LastIndexOf('/') + 1) : path;

        Transform scope = root;
        string prefix = path.Contains("/") ? path.Substring(0, path.LastIndexOf('/')) : "";
        if (!string.IsNullOrEmpty(prefix))
        {
            var p = root.Find(prefix);
            if (p) scope = p;
        }

        return FindDeepChildByName(scope, leaf);
    }

    static Transform FindDeepChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var r = FindDeepChildByName(child, name);
            if (r) return r;
        }
        return null;
    }
}
