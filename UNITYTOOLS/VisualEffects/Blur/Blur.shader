Shader "Custom/Blur"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_Distortion("Distortion", Range(0, 1.0)) = 1.0
		_Factor("Factor", Range(0, 1.0)) = 1.0
		_Mask("Mask", 2D) = "white"{}
    }
    SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Overlay" "DisableBatching"="True"}
		
		ZWrite Off
		ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha

		GrabPass{"_RefractionTex"}
		

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				//float2 screenPos : TEXCOORD1;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
				float4 T2W0 : TEXCOORD2; //w=worldPos.x       //为充分利用插值寄存器，把顶点世界坐标存储在矩阵行的w分量中。
				float4 T2W1 : TEXCOORD3; //w=worldPos.y
				float4 T2W2 : TEXCOORD4; //w=worldPos.z
				SHADOW_COORDS(5)
			};

			sampler2D _RefractionTex;
			float4 _NormalTex_ST;
			float4 _Color;
			float _Distortion;
			float _Factor;

			sampler2D _Mask;
			
			v2f vert (a2v v)
			{
				v2f o;
				//MVP变换--位置裁剪坐标                   (Tips: Android:  o.pos.xy = (0, 0) ~ (1136, 640)   o.pos.w = distance)
				o.pos = UnityObjectToClipPos(v.vertex);  //等同于：mul(UNITY_MATRIX_MVP, v.vertex)    
				//屏幕坐标
				o.screenPos = ComputeGrabScreenPos(o.pos); //ComputeScreenPos返回的值是齐次坐标系下的屏幕坐标值，其范围为[0, w]
				//纹理坐标
				o.uv = v.texcoord;//无偏移
				


				//法线变换到世界坐标
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);//等同于： mul(v.normal, (float3x3)unity_WorldToObject);
				//切线变换到世界坐标
				fixed3 worldTangent= UnityObjectToWorldDir(v.tangent.xyz);
				//BiNormal世界坐标
				fixed3 worldBitangent = normalize(cross(worldNormal, worldTangent)) * v.tangent.w;
				//位置世界坐标
				fixed3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
				
				//Matrix
				o.T2W0 = float4(worldTangent.x, worldBitangent.x, worldNormal.x, worldPos.x);
				o.T2W1 = float4(worldTangent.y, worldBitangent.y, worldNormal.y, worldPos.y);
				o.T2W2 = float4(worldTangent.z, worldBitangent.z, worldNormal.z, worldPos.z);
				
				//ShadowMap坐标
				TRANSFER_SHADOW(o);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//Grab的像素着色
				float3 worldNormalDir = normalize(float3(i.T2W0.z, i.T2W1.z, i.T2W2.z));
				float3 worldPos = float3(i.T2W0.w, i.T2W1.w, i.T2W2.w);
				float3 worldViewDir = -normalize(worldPos - _WorldSpaceCameraPos);
				float viewFactor = dot(worldViewDir, worldNormalDir); //角度系数  

				//___DEBUG__
				//float dtest = i.pos.z; //test what does i.pos.z mean.
				//float r = 0; if(dtest > 0) {r = dtest;}
				//float g = 0; if(dtest < 0) {g = -dtest;}
				//return float4(dtest , 0, 0, 1.0);


				float2 blurFac = _Distortion * tex2D(_Mask, i.uv).r * viewFactor;
				
				float2 screenCoords = i.screenPos.xy / i.screenPos.w;

				float2 screenCoords1 = screenCoords + (blurFac * float2(-1,-1)); 
				float2 screenCoords2 = screenCoords + (blurFac * float2(0,-1)); 
				float2 screenCoords3 = screenCoords + (blurFac * float2(1,-1)); 
				float2 screenCoords4 = screenCoords + (blurFac * float2(-1, 0)); 

				float2 screenCoords6 = screenCoords + (blurFac * float2(1, 0)); 
				float2 screenCoords7 = screenCoords + (blurFac * float2(-1, 1)); 
				float2 screenCoords8 = screenCoords + (blurFac * float2(0, 1)); 
				float2 screenCoords9 = screenCoords + (blurFac * float2(1, 1)); 

				fixed4 color1 = tex2D(_RefractionTex, screenCoords1);
				fixed4 color2 = tex2D(_RefractionTex, screenCoords2);
				fixed4 color3 = tex2D(_RefractionTex, screenCoords3);
				fixed4 color4 = tex2D(_RefractionTex, screenCoords4);
				fixed4 color5 = tex2D(_RefractionTex, screenCoords);
				fixed4 color6 = tex2D(_RefractionTex, screenCoords6);
				fixed4 color7 = tex2D(_RefractionTex, screenCoords7);
				fixed4 color8 = tex2D(_RefractionTex, screenCoords8);
				fixed4 color9 = tex2D(_RefractionTex, screenCoords9);

				//模糊系数
				fixed fac_other = _Factor / 8.0;
				fixed fac_center = 1.0 - _Factor;

				return (color5 * fac_center
					+ color1 * fac_other
					+ color2 * fac_other
					+ color3 * fac_other
					+ color4 * fac_other
					+ color6 * fac_other
					+ color7 * fac_other
					+ color8 * fac_other
					+ color9 * fac_other) * _Color;
			}
			ENDCG
		}
	}
}
