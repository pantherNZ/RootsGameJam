Shader "TreeRootShader" {
    Properties{
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _Colour("Colour Tint", Color) = (1,1,1,1)
    }

    SubShader{
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        GrabPass { }

        Pass {
            CGPROGRAM

			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment
            #pragma target 2.0
            #pragma multi_compile_fog

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _GrabTexture;
			fixed4 _Colour;

            #include "UnityCG.cginc"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				half2 texcoord : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			VertexOutput ComputeVertex(VertexInput vertexInput)
			{
				VertexOutput vertexOutput;

				vertexOutput.vertex = UnityObjectToClipPos(vertexInput.vertex);
				vertexOutput.screenPos = vertexOutput.vertex;
				vertexOutput.texcoord = TRANSFORM_TEX(vertexInput.texcoord, _MainTex);
				vertexOutput.color = vertexInput.color * _Colour;

				return vertexOutput;
			}

			fixed4 Blend(fixed4 a, fixed4 b)
			{
				//fixed4 r = min(a, b);
				//r.a = b.a;
				if( abs( a.a - 0.9991 ) > 0.0001f )
					return b;
				float aMul = 1.0;
				float bMul = 1.8;
				return (a * aMul + b * bMul) / ( aMul + bMul );
			}

			fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
			{
				half4 color = tex2D(_MainTex, vertexOutput.texcoord) * vertexOutput.color;

				float2 grabTexcoord = vertexOutput.screenPos.xy / vertexOutput.screenPos.w;
				grabTexcoord.x = (grabTexcoord.x + 1.0) * .5;
				grabTexcoord.y = (grabTexcoord.y + 1.0) * .5;
				#if UNITY_UV_STARTS_AT_TOP
				grabTexcoord.y = 1.0 - grabTexcoord.y;
				#endif

				color.a = color.a <= 0.001 ? 0.0 : 0.9991f;
				fixed4 grabColor = tex2D(_GrabTexture, grabTexcoord);

				return Blend(grabColor, color);
			}

			ENDCG
        }
    }
}