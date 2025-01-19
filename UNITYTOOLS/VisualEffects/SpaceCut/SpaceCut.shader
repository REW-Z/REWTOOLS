
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SpaceCut"
{
	Properties
	{
		_StartPos("StartPos", Vector) = (0, 0, 0, 0)
		_EndPos("EndPos", Vector) = (0, 0, 0, 0)
		_StartPos2("StartPos2", Vector) = (0, 0, 0, 0)
		_EndPos2("EndPos2", Vector) = (0, 0, 0, 0)
		_StartPos3("StartPos3", Vector) = (0, 0, 0, 0)
		_EndPos3("EndPos3", Vector) = (0, 0, 0, 0)
		_Dislocation("Dislocation", Range(0, 1.0)) = 1.0
		_Dislocation2("Dislocation2", Range(0, 1.0)) = 1.0
		_Dislocation3("Dislocation3", Range(0, 1.0)) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "DisableBatching"="True"}
		
		ZWrite On
		ZTest Off

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
			float _Dislocation;
			float _Dislocation2;
			float _Dislocation3;
			float3 _StartPos;
			float3 _EndPos;
			float3 _StartPos2;
			float3 _EndPos2;
			float3 _StartPos3;
			float3 _EndPos3;

			
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
				float factor = dot(worldViewDir, worldNormalDir);

				//___DEBUG__
				//float dtest = i.pos.z; //test what does i.pos.z mean.
				//float r = 0; if(dtest > 0) {r = dtest;}
				//float g = 0; if(dtest < 0) {g = -dtest;}
				//return float4(dtest , 0, 0, 1.0);


				//float2 bumpCoords = i.uv + float2(0, _Time.y * _Speed);
				//float2 offset = UnpackNormal(tex2D(_NormalTex, bumpCoords * _NormalTex_ST.xy)).xy * _Distortion * tex2D(_Mask, i.uv).r * factor;
				//float2 screenCoords = i.screenPos.xy / i.screenPos.w;
				//screenCoords += offset; 
				

				float2 screenCoords = i.screenPos.xy / i.screenPos.w;
				fixed shadowFac = 1.0;

				float3 sliceDir = float3(_EndPos - _StartPos);
				float3 crs = cross(sliceDir, float3(i.screenPos.xyz / i.screenPos.w - _EndPos));
				float result = dot(float3(0, 0, 1), crs);

				
				float3 sliceDir2 = float3(_EndPos2 - _StartPos2);
				float3 crs2 = cross(sliceDir2, float3(i.screenPos.xyz / i.screenPos.w - _EndPos2));
				float result2 = dot(float3(0, 0, 1), crs2);

				
				float3 sliceDir3 = float3(_EndPos3 - _StartPos3);
				float3 crs3 = cross(sliceDir3, float3(i.screenPos.xyz / i.screenPos.w - _EndPos3));
				float result3 = dot(float3(0, 0, 1), crs3);


				if( result > 0.0 )
				{
					screenCoords += normalize(sliceDir) * _Dislocation;
					//shadowFac *= 0.9;
				}
				else
				{
					screenCoords -= normalize(sliceDir) * _Dislocation;
				}
				if( result2 > 0.0 )
				{
					screenCoords += normalize(sliceDir2) * _Dislocation2;
					//shadowFac *= 0.9;
				}
				else
				{
					screenCoords -= normalize(sliceDir2) * _Dislocation2;
				}
				if( result3 > 0.0 )
				{
					screenCoords += normalize(sliceDir3) * _Dislocation3;
					//shadowFac *= 0.9;
				}
				else
				{
					screenCoords -= normalize(sliceDir3) * _Dislocation3;
				}


				
				fixed4 color = tex2D(_RefractionTex, screenCoords);
				color *= shadowFac;

				//fixed4 color = tex2D(_RefractionTex, screenCoords);

				return color;
			}




			ENDCG
		}
	}
}
