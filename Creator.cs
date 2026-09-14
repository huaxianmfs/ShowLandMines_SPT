using UnityEngine;

namespace ShowLandMines
{
    public static class Creator
    {
        public const string MinefieldObjectName = "ShowLandMines_Minefield";
        public const string SniperZoneObjectName = "ShowLandMines_SniperZone";

        private static Material _redMat;
        private static Material _blueMat;
        private static Material _greenMat;

        private static Material RedMat
        {
            get
            {
                if (_redMat == null)
                    _redMat = CreateMaterial(new Color(1f, 0f, 0f, 0.5f));
                return _redMat;
            }
        }

        private static Material BlueMat
        {
            get
            {
                if (_blueMat == null)
                    _blueMat = CreateMaterial(new Color(0f, 0f, 1f, 0.5f));
                return _blueMat;
            }
        }

        private static Material GreenMat
        {
            get
            {
                if (_greenMat == null)
                    _greenMat = CreateMaterial(new Color(0f, 1f, 0f, 0.5f));
                return _greenMat;
            }
        }

        private static Material CreateMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Standard"));

            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", 5);
            material.SetInt("_DstBlend", 10);
            material.SetInt("_ZWrite", 0);

            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            material.renderQueue = 3000;
            material.color = color;

            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(color.r, color.g, color.b) * 1f);

            return material;
        }

        public static void CreateBox(Transform parent, Vector3 extents, Vector3 center, int color, string objectName)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = objectName;

            go.transform.parent = parent;
            go.transform.localScale = extents;
            go.transform.localPosition = center;
            go.transform.localRotation = Quaternion.identity;

            BoxCollider collider = go.GetComponent<BoxCollider>();
            if (collider != null)
            {
                UnityEngine.Object.Destroy(collider);
            }

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();

            switch (color)
            {
                case 0:
                    renderer.material = RedMat;
                    break;
                case 1:
                    renderer.material = BlueMat;
                    break;
                case 2:
                    renderer.material = GreenMat;
                    break;
            }
        }
    }
}