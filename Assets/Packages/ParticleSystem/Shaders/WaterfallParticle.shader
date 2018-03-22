Shader "Custom/Waterfall" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Scale("Scale", Range(0.01, 0.5)) = 0.24
	}
	SubShader{
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct ParticleData {
				bool isActive;
				int id;
				float life;
				float2 position;
				float2 velocity;
			};

			struct v2g {
				float4 pos : POSITION;
				float4 col : COLOR;
				float scale : TEXCOORD0;
			};

			struct g2f {
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float2 uv : TEXCOORD0;
			};

			StructuredBuffer<ParticleData> _ParticlesBuffer;
			sampler2D _MainTex;
			float _Scale;
			float4 _Color;

			v2g vert(uint id: SV_VertexID) {
				v2g o;
				o.pos = float4(_ParticlesBuffer[id].position.xy, 0, 1);

				if (_ParticlesBuffer[id].isActive) {
					o.scale = _Scale;
					o.col = _Color * _ParticlesBuffer[id].life;
				} else {
					o.scale = 0;
					o.pos = float4(0xffffff, 0xffffff, 0, 1);
				}
				
				// o.col = _Color * _ParticlesBuffer[id].life;

				return o;
			}

			[maxvertexcount(4)]
			void geom(point v2g input[1], inout TriangleStream<g2f> outStream) {
				g2f o;

				float4 pos = input[0].pos;

				float4x4 billboardMatrix = UNITY_MATRIX_V;
				billboardMatrix._m03 = billboardMatrix._m13 = billboardMatrix._m23 = billboardMatrix._m33 = 0;

				for (int x = 0; x < 2; x++) {
					for (int y = 0; y < 2; y++) {
						float2 uv = float2(x, y);
						o.uv = uv;

						o.pos = pos + mul(float4((uv * 2 - float2(1, 1)) * input[0].scale, 0, 1), billboardMatrix);
						o.pos = mul(UNITY_MATRIX_VP, o.pos);

						o.col = input[0].col;

						outStream.Append(o);
					}
				}

				outStream.RestartStrip();
			}

			fixed4 frag(g2f i) : SV_Target{
				fixed4 col = tex2D(_MainTex, i.uv) * i.col;
				return col;
			}
				
			ENDCG
		}
	}
}
