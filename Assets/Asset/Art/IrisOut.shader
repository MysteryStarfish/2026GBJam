Shader "Custom/IrisOut"
{
    Properties
    {
        // 加入這行，加上 [PerRendererData] 標籤讓 UI 系統自己塞入預設圖，不顯示在面板上
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {} 
        
        _Color ("Background Color", Color) = (0,0,0,1)
        _Center ("Center (Viewport)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Range(0, 2)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay+1000" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex; // 宣告 _MainTex 變數對應 Properties
            float4 _Color;
            float2 _Center;
            float _Radius;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 這裡的邏輯不變
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 uv = i.uv;
                uv.x *= aspect;
                
                float2 center = _Center;
                center.x *= aspect;

                float dist = distance(uv, center);
                float alpha = step(_Radius, dist);
                
                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}