//6000_0_0b13
//↓コメントアウトしないと動かなかったはずだけど今外しても動いてる
// //#line 1 ""
// static const int _SHADOWS_SOFT_LOW = 0 ;
// static const int _SHADOWS_SOFT_MEDIUM = 0 ;
// static const int _SHADOWS_SOFT_HIGH = 0 ;
// static const int _DBUFFER_MRT1 = 0 ;
// static const int _DBUFFER_MRT2 = 0 ;
// static const int _DBUFFER_MRT3 = 0 ;
// static const int _LIGHT_COOKIES = 0 ;
// static const int _WRITE_RENDERING_LAYERS = 0 ;
// static const int DEBUG_DISPLAY = 0 ;
// static const int _MAIN_LIGHT_SHADOWS = 0 ;
// static const int _MAIN_LIGHT_SHADOWS_SCREEN = 0 ;
// static const int _ADDITIONAL_LIGHTS_VERTEX = 0 ;
// static const int _ADDITIONAL_LIGHTS = 0 ;
// static const int EVALUATE_SH_MIXED = 0 ;
// static const int EVALUATE_SH_VERTEX = 0 ;
// static const int LIGHTMAP_SHADOW_MIXING = 0 ;
// static const int SHADOWS_SHADOWMASK = 0 ;
// static const int DIRLIGHTMAP_COMBINED = 0 ;
// static const int LIGHTMAP_ON = 0 ;
// static const int DYNAMICLIGHTMAP_ON = 0 ;
// static const int USE_LEGACY_LIGHTMAPS = 0 ;
// static const int LOD_FADE_CROSSFADE = 0 ;
// static const int PROBE_VOLUMES_L2 = 0 ;
// static const int DOTS_INSTANCING_ON = 0 ;
// static const int FOG_LINEAR = 0 ;
// static const int FOG_EXP = 0 ;
// static const int FOG_EXP2 = 0 ;
// static const int INSTANCING_ON = 0 ;
// static const int _SURFACE_TYPE_TRANSPARENT = 0 ;
// static const int _ALPHATEST_ON = 0 ;
// static const int _ALPHAPREMULTIPLY_ON = 0 ;
// static const int _ALPHAMODULATE_ON = 0 ;
// static const int _EMISSION = 0 ;
// static const int _METALLICSPECGLOSSMAP = 0 ;
// static const int _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A = 0 ;
// static const int _OCCLUSIONMAP = 0 ;
// static const int _SPECULARHIGHLIGHTS_OFF = 0 ;
// static const int _ENVIRONMENTREFLECTIONS_OFF = 0 ;
// static const int _SPECULAR_SETUP = 0 ;
// static const int _NORMALMAP = 0 ;
// static const int _PARALLAXMAP = 0 ;
// static const int _RECEIVE_SHADOWS_OFF = 0 ;
// static const int _DETAIL_MULX2 = 0 ;
// static const int _DETAIL_SCALED = 0 ;
// static const int UNITY_NO_DXT5nm = 0 ;
// static const int UNITY_FRAMEBUFFER_FETCH_AVAILABLE = 0 ;
// static const int UNITY_METAL_SHADOWS_USE_POINT_FILTERING = 0 ;
// static const int UNITY_NO_SCREENSPACE_SHADOWS = 0 ;
// static const int UNITY_PBS_USE_BRDF2 = 0 ;
// static const int UNITY_PBS_USE_BRDF3 = 0 ;
// static const int UNITY_HARDWARE_TIER1 = 0 ;
// static const int UNITY_HARDWARE_TIER2 = 0 ;
// static const int UNITY_HARDWARE_TIER3 = 0 ;
// static const int UNITY_COLORSPACE_GAMMA = 0 ;
// static const int UNITY_LIGHTMAP_DLDR_ENCODING = 0 ;
// static const int UNITY_LIGHTMAP_RGBM_ENCODING = 0 ;
// static const int UNITY_VIRTUAL_TEXTURING = 0 ;
// static const int UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION = 0 ;
// static const int UNITY_ASTC_NORMALMAP_ENCODING = 0 ;
// static const int SHADER_API_GLES30 = 0 ;
// static const int UNITY_UNIFIED_SHADER_PRECISION_MODEL = 0 ;
// //#line 117
// cbuffer UnityDynamicKeywords {
// int _FOVEATED_RENDERING_NON_UNIFORM_RASTER ;
// } ;
//#line 4 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/Random.hlsl"
float Hash ( uint s )
{
s = s ^ 2747636419u ;
s = s * 2654435769u ;
s = s ^ ( s >> 16 ) ;
s = s * 2654435769u ;
s = s ^ ( s >> 16 ) ;
s = s * 2654435769u ;
return float ( s ) * rcp ( 4294967296.0 ) ;
}
//#line 16
uint JenkinsHash ( uint x )
{
x += ( x << 10u ) ;
x ^= ( x >> 6u ) ;
x += ( x << 3u ) ;
x ^= ( x >> 11u ) ;
x += ( x << 15u ) ;
return x ;
}
//#line 27
uint JenkinsHash ( uint2 v )
{
return JenkinsHash ( v . x ^ JenkinsHash ( v . y ) ) ;
}

uint JenkinsHash ( uint3 v )
{
return JenkinsHash ( v . x ^ JenkinsHash ( v . yz ) ) ;
}

uint JenkinsHash ( uint4 v )
{
return JenkinsHash ( v . x ^ JenkinsHash ( v . yzw ) ) ;
}
//#line 44
float ConstructFloat ( int m ) {
const int ieeeMantissa = 0x007FFFFF ;
const int ieeeOne = 0x3F800000 ;

m &= ieeeMantissa ;
m |= ieeeOne ;

float f = asfloat ( m ) ;
return f - 1 ;
}

float ConstructFloat ( uint m )
{
return ConstructFloat ( asint ( m ) ) ;
}
//#line 62
float GenerateHashedRandomFloat ( uint x )
{
return ConstructFloat ( JenkinsHash ( x ) ) ;
}

float GenerateHashedRandomFloat ( uint2 v )
{
return ConstructFloat ( JenkinsHash ( v ) ) ;
}

float GenerateHashedRandomFloat ( uint3 v )
{
return ConstructFloat ( JenkinsHash ( v ) ) ;
}

float GenerateHashedRandomFloat ( uint4 v )
{
return ConstructFloat ( JenkinsHash ( v ) ) ;
}

float2 InitRandom ( float2 input )
{
float2 r ;
r . x = Hash ( uint ( input . x * 0xFFFFFFFFu ) ) ;
r . y = Hash ( uint ( input . y * 0xFFFFFFFFu ) ) ;

return r ;
}
//#line 93
float InterleavedGradientNoise ( float2 pixCoord , int frameCount )
{
const float3 magic = float3 ( 0.06711056f , 0.00583715f , 52.9829189f ) ;
float2 frameMagicScale = float2 ( 2.083f , 4.867f ) ;
pixCoord += frameCount * frameMagicScale ;
return frac ( magic . z * frac ( dot ( pixCoord , magic . xy ) ) ) ;
}
//#line 102
uint XorShift ( inout uint rngState )
{
rngState ^= rngState << 13 ;
rngState ^= rngState >> 17 ;
rngState ^= rngState << 5 ;
return rngState ;
}
//#line 8 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/CommonDeprecated.hlsl"
void LODDitheringTransition ( uint3 fadeMaskSeed , float ditherFactor )
{
ditherFactor = ditherFactor < 0.0 ? 1 + ditherFactor : ditherFactor ;

float p = GenerateHashedRandomFloat ( fadeMaskSeed ) ;
p = ( ditherFactor >= 0.5 ) ? p : 1 - p ;
clip ( ditherFactor - p ) ;
}
//#line 444 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
uint BitFieldExtract ( uint data , uint offset , uint numBits )
{
uint mask = ( 1u << numBits ) - 1u ;
return ( data >> offset ) & mask ;
}
//#line 455
int BitFieldExtractSignExtend ( int data , uint offset , uint numBits )
{
int shifted = data >> offset ;
int signBit = shifted & ( 1u << ( numBits - 1u ) ) ;
uint mask = ( 1u << numBits ) - 1u ;

return - signBit | ( shifted & mask ) ;
}
//#line 467
uint BitFieldInsert ( uint mask , uint src , uint dst )
{
return ( src & mask ) | ( dst & ~ mask ) ;
}
//#line 473
bool IsBitSet ( uint data , uint offset )
{
return BitFieldExtract ( data , offset , 1u ) != 0 ;
}

void SetBit ( inout uint data , uint offset )
{
data |= 1u << offset ;
}

void ClearBit ( inout uint data , uint offset )
{
data &= ~ ( 1u << offset ) ;
}

void ToggleBit ( inout uint data , uint offset )
{
data ^= 1u << offset ;
}
//#line 495
min16float WaveReadLaneFirst ( min16float scalarValue ) { return scalarValue ; } min16float2 WaveReadLaneFirst ( min16float2 scalarValue ) { return scalarValue ; } min16float3 WaveReadLaneFirst ( min16float3 scalarValue ) { return scalarValue ; } min16float4 WaveReadLaneFirst ( min16float4 scalarValue ) { return scalarValue ; } float WaveReadLaneFirst ( float scalarValue ) { return scalarValue ; } float2 WaveReadLaneFirst ( float2 scalarValue ) { return scalarValue ; } float3 WaveReadLaneFirst ( float3 scalarValue ) { return scalarValue ; } float4 WaveReadLaneFirst ( float4 scalarValue ) { return scalarValue ; }
int WaveReadLaneFirst ( int scalarValue ) { return scalarValue ; } int2 WaveReadLaneFirst ( int2 scalarValue ) { return scalarValue ; } int3 WaveReadLaneFirst ( int3 scalarValue ) { return scalarValue ; } int4 WaveReadLaneFirst ( int4 scalarValue ) { return scalarValue ; } uint WaveReadLaneFirst ( uint scalarValue ) { return scalarValue ; } uint2 WaveReadLaneFirst ( uint2 scalarValue ) { return scalarValue ; } uint3 WaveReadLaneFirst ( uint3 scalarValue ) { return scalarValue ; } uint4 WaveReadLaneFirst ( uint4 scalarValue ) { return scalarValue ; }
//#line 500
int Mul24 ( int a , int b ) { return a * b ; } int2 Mul24 ( int2 a , int2 b ) { return a * b ; } int3 Mul24 ( int3 a , int3 b ) { return a * b ; } int4 Mul24 ( int4 a , int4 b ) { return a * b ; } uint Mul24 ( uint a , uint b ) { return a * b ; } uint2 Mul24 ( uint2 a , uint2 b ) { return a * b ; } uint3 Mul24 ( uint3 a , uint3 b ) { return a * b ; } uint4 Mul24 ( uint4 a , uint4 b ) { return a * b ; }
//#line 504
int Mad24 ( int a , int b , int c ) { return a * b + c ; } int2 Mad24 ( int2 a , int2 b , int2 c ) { return a * b + c ; } int3 Mad24 ( int3 a , int3 b , int3 c ) { return a * b + c ; } int4 Mad24 ( int4 a , int4 b , int4 c ) { return a * b + c ; } uint Mad24 ( uint a , uint b , uint c ) { return a * b + c ; } uint2 Mad24 ( uint2 a , uint2 b , uint2 c ) { return a * b + c ; } uint3 Mad24 ( uint3 a , uint3 b , uint3 c ) { return a * b + c ; } uint4 Mad24 ( uint4 a , uint4 b , uint4 c ) { return a * b + c ; }
//#line 508
min16float Min3 ( min16float a , min16float b , min16float c ) { return min ( min ( a , b ) , c ) ; } min16float2 Min3 ( min16float2 a , min16float2 b , min16float2 c ) { return min ( min ( a , b ) , c ) ; } min16float3 Min3 ( min16float3 a , min16float3 b , min16float3 c ) { return min ( min ( a , b ) , c ) ; } min16float4 Min3 ( min16float4 a , min16float4 b , min16float4 c ) { return min ( min ( a , b ) , c ) ; } float Min3 ( float a , float b , float c ) { return min ( min ( a , b ) , c ) ; } float2 Min3 ( float2 a , float2 b , float2 c ) { return min ( min ( a , b ) , c ) ; } float3 Min3 ( float3 a , float3 b , float3 c ) { return min ( min ( a , b ) , c ) ; } float4 Min3 ( float4 a , float4 b , float4 c ) { return min ( min ( a , b ) , c ) ; }
int Min3 ( int a , int b , int c ) { return min ( min ( a , b ) , c ) ; } int2 Min3 ( int2 a , int2 b , int2 c ) { return min ( min ( a , b ) , c ) ; } int3 Min3 ( int3 a , int3 b , int3 c ) { return min ( min ( a , b ) , c ) ; } int4 Min3 ( int4 a , int4 b , int4 c ) { return min ( min ( a , b ) , c ) ; } uint Min3 ( uint a , uint b , uint c ) { return min ( min ( a , b ) , c ) ; } uint2 Min3 ( uint2 a , uint2 b , uint2 c ) { return min ( min ( a , b ) , c ) ; } uint3 Min3 ( uint3 a , uint3 b , uint3 c ) { return min ( min ( a , b ) , c ) ; } uint4 Min3 ( uint4 a , uint4 b , uint4 c ) { return min ( min ( a , b ) , c ) ; }
min16float Max3 ( min16float a , min16float b , min16float c ) { return max ( max ( a , b ) , c ) ; } min16float2 Max3 ( min16float2 a , min16float2 b , min16float2 c ) { return max ( max ( a , b ) , c ) ; } min16float3 Max3 ( min16float3 a , min16float3 b , min16float3 c ) { return max ( max ( a , b ) , c ) ; } min16float4 Max3 ( min16float4 a , min16float4 b , min16float4 c ) { return max ( max ( a , b ) , c ) ; } float Max3 ( float a , float b , float c ) { return max ( max ( a , b ) , c ) ; } float2 Max3 ( float2 a , float2 b , float2 c ) { return max ( max ( a , b ) , c ) ; } float3 Max3 ( float3 a , float3 b , float3 c ) { return max ( max ( a , b ) , c ) ; } float4 Max3 ( float4 a , float4 b , float4 c ) { return max ( max ( a , b ) , c ) ; }
int Max3 ( int a , int b , int c ) { return max ( max ( a , b ) , c ) ; } int2 Max3 ( int2 a , int2 b , int2 c ) { return max ( max ( a , b ) , c ) ; } int3 Max3 ( int3 a , int3 b , int3 c ) { return max ( max ( a , b ) , c ) ; } int4 Max3 ( int4 a , int4 b , int4 c ) { return max ( max ( a , b ) , c ) ; } uint Max3 ( uint a , uint b , uint c ) { return max ( max ( a , b ) , c ) ; } uint2 Max3 ( uint2 a , uint2 b , uint2 c ) { return max ( max ( a , b ) , c ) ; } uint3 Max3 ( uint3 a , uint3 b , uint3 c ) { return max ( max ( a , b ) , c ) ; } uint4 Max3 ( uint4 a , uint4 b , uint4 c ) { return max ( max ( a , b ) , c ) ; }
//#line 514
min16float Avg3 ( min16float a , min16float b , min16float c ) { return ( a + b + c ) * 0.33333333 ; } min16float2 Avg3 ( min16float2 a , min16float2 b , min16float2 c ) { return ( a + b + c ) * 0.33333333 ; } min16float3 Avg3 ( min16float3 a , min16float3 b , min16float3 c ) { return ( a + b + c ) * 0.33333333 ; } min16float4 Avg3 ( min16float4 a , min16float4 b , min16float4 c ) { return ( a + b + c ) * 0.33333333 ; } float Avg3 ( float a , float b , float c ) { return ( a + b + c ) * 0.33333333 ; } float2 Avg3 ( float2 a , float2 b , float2 c ) { return ( a + b + c ) * 0.33333333 ; } float3 Avg3 ( float3 a , float3 b , float3 c ) { return ( a + b + c ) * 0.33333333 ; } float4 Avg3 ( float4 a , float4 b , float4 c ) { return ( a + b + c ) * 0.33333333 ; }
//#line 517
float2 GetQuadOffset ( int2 screenPos )
{
return float2 ( float ( screenPos . x & 1 ) * 2.0 - 1.0 , float ( screenPos . y & 1 ) * 2.0 - 1.0 ) ;
}
//#line 523
float QuadReadAcrossX ( float value , int2 screenPos )
{
return value - ( ddx_fine ( value ) * ( float ( screenPos . x & 1 ) * 2.0 - 1.0 ) ) ;
}

float QuadReadAcrossY ( float value , int2 screenPos )
{
return value - ( ddy_fine ( value ) * ( float ( screenPos . y & 1 ) * 2.0 - 1.0 ) ) ;
}

float QuadReadAcrossDiagonal ( float value , int2 screenPos )
{
float2 quadDir = GetQuadOffset ( screenPos ) ;
float dX = ddx_fine ( value ) ;
float X = value - ( dX * quadDir . x ) ;
return X - ( ddy_fine ( X ) * quadDir . y ) ;
}
//#line 542
float3 QuadReadFloat3AcrossX ( float3 val , int2 positionSS )
{
return float3 ( QuadReadAcrossX ( val . x , positionSS ) , QuadReadAcrossX ( val . y , positionSS ) , QuadReadAcrossX ( val . z , positionSS ) ) ;
}

float4 QuadReadFloat4AcrossX ( float4 val , int2 positionSS )
{
return float4 ( QuadReadAcrossX ( val . x , positionSS ) , QuadReadAcrossX ( val . y , positionSS ) , QuadReadAcrossX ( val . z , positionSS ) , QuadReadAcrossX ( val . w , positionSS ) ) ;
}

float3 QuadReadFloat3AcrossY ( float3 val , int2 positionSS )
{
return float3 ( QuadReadAcrossY ( val . x , positionSS ) , QuadReadAcrossY ( val . y , positionSS ) , QuadReadAcrossY ( val . z , positionSS ) ) ;
}

float4 QuadReadFloat4AcrossY ( float4 val , int2 positionSS )
{
return float4 ( QuadReadAcrossY ( val . x , positionSS ) , QuadReadAcrossY ( val . y , positionSS ) , QuadReadAcrossY ( val . z , positionSS ) , QuadReadAcrossY ( val . w , positionSS ) ) ;
}

float3 QuadReadFloat3AcrossDiagonal ( float3 val , int2 positionSS )
{
return float3 ( QuadReadAcrossDiagonal ( val . x , positionSS ) , QuadReadAcrossDiagonal ( val . y , positionSS ) , QuadReadAcrossDiagonal ( val . z , positionSS ) ) ;
}

float4 QuadReadFloat4AcrossDiagonal ( float4 val , int2 positionSS )
{
return float4 ( QuadReadAcrossDiagonal ( val . x , positionSS ) , QuadReadAcrossDiagonal ( val . y , positionSS ) , QuadReadAcrossDiagonal ( val . z , positionSS ) , QuadReadAcrossDiagonal ( val . w , positionSS ) ) ;
}

void Swap ( inout min16float a , inout min16float b ) { min16float t = a ; a = b ; b = t ; } void Swap ( inout min16float2 a , inout min16float2 b ) { min16float2 t = a ; a = b ; b = t ; } void Swap ( inout min16float3 a , inout min16float3 b ) { min16float3 t = a ; a = b ; b = t ; } void Swap ( inout min16float4 a , inout min16float4 b ) { min16float4 t = a ; a = b ; b = t ; } void Swap ( inout float a , inout float b ) { float t = a ; a = b ; b = t ; } void Swap ( inout float2 a , inout float2 b ) { float2 t = a ; a = b ; b = t ; } void Swap ( inout float3 a , inout float3 b ) { float3 t = a ; a = b ; b = t ; } void Swap ( inout float4 a , inout float4 b ) { float4 t = a ; a = b ; b = t ; } void Swap ( inout int a , inout int b ) { int t = a ; a = b ; b = t ; } void Swap ( inout int2 a , inout int2 b ) { int2 t = a ; a = b ; b = t ; } void Swap ( inout int3 a , inout int3 b ) { int3 t = a ; a = b ; b = t ; } void Swap ( inout int4 a , inout int4 b ) { int4 t = a ; a = b ; b = t ; } void Swap ( inout uint a , inout uint b ) { uint t = a ; a = b ; b = t ; } void Swap ( inout uint2 a , inout uint2 b ) { uint2 t = a ; a = b ; b = t ; } void Swap ( inout uint3 a , inout uint3 b ) { uint3 t = a ; a = b ; b = t ; } void Swap ( inout uint4 a , inout uint4 b ) { uint4 t = a ; a = b ; b = t ; } void Swap ( inout bool a , inout bool b ) { bool t = a ; a = b ; b = t ; } void Swap ( inout bool2 a , inout bool2 b ) { bool2 t = a ; a = b ; b = t ; } void Swap ( inout bool3 a , inout bool3 b ) { bool3 t = a ; a = b ; b = t ; } void Swap ( inout bool4 a , inout bool4 b ) { bool4 t = a ; a = b ; b = t ; }
//#line 582
float CubeMapFaceID ( float3 dir )
{
float faceID ;

if ( abs ( dir . z ) >= abs ( dir . x ) && abs ( dir . z ) >= abs ( dir . y ) )
{
faceID = ( dir . z < 0.0 ) ? 5 : 4 ;
}
else if ( abs ( dir . y ) >= abs ( dir . x ) )
{
faceID = ( dir . y < 0.0 ) ? 3 : 2 ;
}
else
{
faceID = ( dir . x < 0.0 ) ? 1 : 0 ;
}

return faceID ;
}
//#line 604
bool IsNaN ( float x )
{
return ( asuint ( x ) & 0x7FFFFFFF ) > 0x7F800000 ;
}

bool AnyIsNaN ( float2 v )
{
return ( IsNaN ( v . x ) || IsNaN ( v . y ) ) ;
}

bool AnyIsNaN ( float3 v )
{
return ( IsNaN ( v . x ) || IsNaN ( v . y ) || IsNaN ( v . z ) ) ;
}

bool AnyIsNaN ( float4 v )
{
return ( IsNaN ( v . x ) || IsNaN ( v . y ) || IsNaN ( v . z ) || IsNaN ( v . w ) ) ;
}

bool IsInf ( float x )
{
return ( asuint ( x ) & 0x7FFFFFFF ) == 0x7F800000 ;
}

bool AnyIsInf ( float2 v )
{
return ( IsInf ( v . x ) || IsInf ( v . y ) ) ;
}

bool AnyIsInf ( float3 v )
{
return ( IsInf ( v . x ) || IsInf ( v . y ) || IsInf ( v . z ) ) ;
}

bool AnyIsInf ( float4 v )
{
return ( IsInf ( v . x ) || IsInf ( v . y ) || IsInf ( v . z ) || IsInf ( v . w ) ) ;
}

bool IsFinite ( float x )
{
return ( asuint ( x ) & 0x7F800000 ) != 0x7F800000 ;
}

float SanitizeFinite ( float x )
{
return IsFinite ( x ) ? x : 0 ;
}

bool IsPositiveFinite ( float x )
{
return asuint ( x ) < 0x7F800000 ;
}

float SanitizePositiveFinite ( float x )
{
return IsPositiveFinite ( x ) ? x : 0 ;
}
//#line 668
float DegToRad ( float deg )
{
return deg * ( 3.14159265358979323846 / 180.0 ) ;
}

float RadToDeg ( float rad )
{
return rad * ( 180.0 / 3.14159265358979323846 ) ;
}
//#line 679
min16float Sq ( min16float x ) { return ( x ) * ( x ) ; } min16float2 Sq ( min16float2 x ) { return ( x ) * ( x ) ; } min16float3 Sq ( min16float3 x ) { return ( x ) * ( x ) ; } min16float4 Sq ( min16float4 x ) { return ( x ) * ( x ) ; } float Sq ( float x ) { return ( x ) * ( x ) ; } float2 Sq ( float2 x ) { return ( x ) * ( x ) ; } float3 Sq ( float3 x ) { return ( x ) * ( x ) ; } float4 Sq ( float4 x ) { return ( x ) * ( x ) ; }
int Sq ( int x ) { return ( x ) * ( x ) ; } int2 Sq ( int2 x ) { return ( x ) * ( x ) ; } int3 Sq ( int3 x ) { return ( x ) * ( x ) ; } int4 Sq ( int4 x ) { return ( x ) * ( x ) ; } uint Sq ( uint x ) { return ( x ) * ( x ) ; } uint2 Sq ( uint2 x ) { return ( x ) * ( x ) ; } uint3 Sq ( uint3 x ) { return ( x ) * ( x ) ; } uint4 Sq ( uint4 x ) { return ( x ) * ( x ) ; }

bool IsPower2 ( uint x )
{
return ( x & ( x - 1 ) ) == 0 ;
}
//#line 689
float FastACosPos ( float inX )
{
float x = abs ( inX ) ;
float res = ( 0.0468878 * x + - 0.203471 ) * x + 1.570796 ;
res *= sqrt ( 1.0 - x ) ;

return res ;
}
//#line 701
float FastACos ( float inX )
{
float res = FastACosPos ( inX ) ;

return ( inX >= 0 ) ? res : 3.14159265358979323846 - res ;
}
//#line 711
float FastASin ( float x )
{
return 1.57079632679489661923 - FastACos ( x ) ;
}
//#line 720
float FastATanPos ( float x )
{
float t0 = ( x < 1.0 ) ? x : 1.0 / x ;
float t1 = t0 * t0 ;
float poly = 0.0872929 ;
poly = - 0.301895 + poly * t1 ;
poly = 1.0 + poly * t1 ;
poly = poly * t0 ;
return ( x < 1.0 ) ? poly : 1.57079632679489661923 - poly ;
}
//#line 733
float FastATan ( float x )
{
float t0 = FastATanPos ( abs ( x ) ) ;
return ( x < 0.0 ) ? - t0 : t0 ;
}

float FastAtan2 ( float y , float x )
{
return FastATan ( y / x ) + float ( y >= 0.0 ? 3.14159265358979323846 : - 3.14159265358979323846 ) * ( x < 0.0 ) ;
}
//#line 745
uint FastLog2 ( uint x )
{
return firstbithigh ( x ) ;
}
//#line 755
min16float PositivePow ( min16float base , min16float power ) { return pow ( abs ( base ) , power ) ; } min16float2 PositivePow ( min16float2 base , min16float2 power ) { return pow ( abs ( base ) , power ) ; } min16float3 PositivePow ( min16float3 base , min16float3 power ) { return pow ( abs ( base ) , power ) ; } min16float4 PositivePow ( min16float4 base , min16float4 power ) { return pow ( abs ( base ) , power ) ; } float PositivePow ( float base , float power ) { return pow ( abs ( base ) , power ) ; } float2 PositivePow ( float2 base , float2 power ) { return pow ( abs ( base ) , power ) ; } float3 PositivePow ( float3 base , float3 power ) { return pow ( abs ( base ) , power ) ; } float4 PositivePow ( float4 base , float4 power ) { return pow ( abs ( base ) , power ) ; }
//#line 788
float SafePositivePow ( float base , float power ) { return pow ( max ( abs ( base ) , float ( 5.960464478e-8 ) ) , power ) ; } float2 SafePositivePow ( float2 base , float2 power ) { return pow ( max ( abs ( base ) , float ( 5.960464478e-8 ) ) , power ) ; } float3 SafePositivePow ( float3 base , float3 power ) { return pow ( max ( abs ( base ) , float ( 5.960464478e-8 ) ) , power ) ; } float4 SafePositivePow ( float4 base , float4 power ) { return pow ( max ( abs ( base ) , float ( 5.960464478e-8 ) ) , power ) ; }
min16float SafePositivePow ( min16float base , min16float power ) { return pow ( max ( abs ( base ) , min16float ( 4.8828125e-4 ) ) , power ) ; } min16float2 SafePositivePow ( min16float2 base , min16float2 power ) { return pow ( max ( abs ( base ) , min16float ( 4.8828125e-4 ) ) , power ) ; } min16float3 SafePositivePow ( min16float3 base , min16float3 power ) { return pow ( max ( abs ( base ) , min16float ( 4.8828125e-4 ) ) , power ) ; } min16float4 SafePositivePow ( min16float4 base , min16float4 power ) { return pow ( max ( abs ( base ) , min16float ( 4.8828125e-4 ) ) , power ) ; }
//#line 792
float SafePositivePow_float ( float base , float power ) { return pow ( max ( abs ( base ) , float ( 5.960464478e-8 ) ) , power ) ; } float2 SafePositivePow_float ( float2 base , float2 power ) { return pow ( max ( abs ( base ) , float ( 5.960464478e-8 ) ) , power ) ; } float3 SafePositivePow_float ( float3 base , float3 power ) { return pow ( max ( abs ( base ) , float ( 5.960464478e-8 ) ) , power ) ; } float4 SafePositivePow_float ( float4 base , float4 power ) { return pow ( max ( abs ( base ) , float ( 5.960464478e-8 ) ) , power ) ; }
min16float SafePositivePow_half ( min16float base , min16float power ) { return pow ( max ( abs ( base ) , min16float ( 4.8828125e-4 ) ) , power ) ; } min16float2 SafePositivePow_half ( min16float2 base , min16float2 power ) { return pow ( max ( abs ( base ) , min16float ( 4.8828125e-4 ) ) , power ) ; } min16float3 SafePositivePow_half ( min16float3 base , min16float3 power ) { return pow ( max ( abs ( base ) , min16float ( 4.8828125e-4 ) ) , power ) ; } min16float4 SafePositivePow_half ( min16float4 base , min16float4 power ) { return pow ( max ( abs ( base ) , min16float ( 4.8828125e-4 ) ) , power ) ; }

float Eps_float ( ) { return 5.960464478e-8 ; }
float Min_float ( ) { return 1.175494351e-38 ; }
float Max_float ( ) { return 3.402823466e+38 ; }
half Eps_half ( ) { return 4.8828125e-4 ; }
half Min_half ( ) { return 6.103515625e-5 ; }
half Max_half ( ) { return 65504.0 ; }
//#line 804
bool NearlyEqual ( float a , float b , float epsilon )
{
return abs ( a - b ) / ( abs ( a ) + abs ( b ) ) < epsilon ;
}

float NearlyEqual_Float ( float a , float b ) { return abs ( a - b ) / ( abs ( a ) + abs ( b ) ) < float ( 5.960464478e-8 ) ; } float2 NearlyEqual_Float ( float2 a , float2 b ) { return abs ( a - b ) / ( abs ( a ) + abs ( b ) ) < float ( 5.960464478e-8 ) ; } float3 NearlyEqual_Float ( float3 a , float3 b ) { return abs ( a - b ) / ( abs ( a ) + abs ( b ) ) < float ( 5.960464478e-8 ) ; } float4 NearlyEqual_Float ( float4 a , float4 b ) { return abs ( a - b ) / ( abs ( a ) + abs ( b ) ) < float ( 5.960464478e-8 ) ; }
min16float NearlyEqual_Half ( min16float a , min16float b ) { return abs ( a - b ) / ( abs ( a ) + abs ( b ) ) < min16float ( 4.8828125e-4 ) ; } min16float2 NearlyEqual_Half ( min16float2 a , min16float2 b ) { return abs ( a - b ) / ( abs ( a ) + abs ( b ) ) < min16float ( 4.8828125e-4 ) ; } min16float3 NearlyEqual_Half ( min16float3 a , min16float3 b ) { return abs ( a - b ) / ( abs ( a ) + abs ( b ) ) < min16float ( 4.8828125e-4 ) ; } min16float4 NearlyEqual_Half ( min16float4 a , min16float4 b ) { return abs ( a - b ) / ( abs ( a ) + abs ( b ) ) < min16float ( 4.8828125e-4 ) ; }
//#line 814
float CopySign ( float x , float s , bool ignoreNegZero = true )
{
if ( ignoreNegZero )
{
return ( s >= 0 ) ? abs ( x ) : - abs ( x ) ;
}
else
{
uint negZero = 0x80000000u ;
uint signBit = negZero & asuint ( s ) ;
return asfloat ( BitFieldInsert ( negZero , signBit , asuint ( x ) ) ) ;
}
}
//#line 835
float FastSign ( float s , bool ignoreNegZero = true )
{
return CopySign ( 1.0 , s , ignoreNegZero ) ;
}
//#line 844
float3 Orthonormalize ( float3 tangent , float3 normal )
{

return normalize ( tangent - dot ( tangent , normal ) * normal ) ;
}
//#line 851
min16float Remap01 ( min16float x , min16float rcpLength , min16float startTimesRcpLength ) { return saturate ( x * rcpLength - startTimesRcpLength ) ; } min16float2 Remap01 ( min16float2 x , min16float2 rcpLength , min16float2 startTimesRcpLength ) { return saturate ( x * rcpLength - startTimesRcpLength ) ; } min16float3 Remap01 ( min16float3 x , min16float3 rcpLength , min16float3 startTimesRcpLength ) { return saturate ( x * rcpLength - startTimesRcpLength ) ; } min16float4 Remap01 ( min16float4 x , min16float4 rcpLength , min16float4 startTimesRcpLength ) { return saturate ( x * rcpLength - startTimesRcpLength ) ; } float Remap01 ( float x , float rcpLength , float startTimesRcpLength ) { return saturate ( x * rcpLength - startTimesRcpLength ) ; } float2 Remap01 ( float2 x , float2 rcpLength , float2 startTimesRcpLength ) { return saturate ( x * rcpLength - startTimesRcpLength ) ; } float3 Remap01 ( float3 x , float3 rcpLength , float3 startTimesRcpLength ) { return saturate ( x * rcpLength - startTimesRcpLength ) ; } float4 Remap01 ( float4 x , float4 rcpLength , float4 startTimesRcpLength ) { return saturate ( x * rcpLength - startTimesRcpLength ) ; }
//#line 854
min16float Remap10 ( min16float x , min16float rcpLength , min16float endTimesRcpLength ) { return saturate ( endTimesRcpLength - x * rcpLength ) ; } min16float2 Remap10 ( min16float2 x , min16float2 rcpLength , min16float2 endTimesRcpLength ) { return saturate ( endTimesRcpLength - x * rcpLength ) ; } min16float3 Remap10 ( min16float3 x , min16float3 rcpLength , min16float3 endTimesRcpLength ) { return saturate ( endTimesRcpLength - x * rcpLength ) ; } min16float4 Remap10 ( min16float4 x , min16float4 rcpLength , min16float4 endTimesRcpLength ) { return saturate ( endTimesRcpLength - x * rcpLength ) ; } float Remap10 ( float x , float rcpLength , float endTimesRcpLength ) { return saturate ( endTimesRcpLength - x * rcpLength ) ; } float2 Remap10 ( float2 x , float2 rcpLength , float2 endTimesRcpLength ) { return saturate ( endTimesRcpLength - x * rcpLength ) ; } float3 Remap10 ( float3 x , float3 rcpLength , float3 endTimesRcpLength ) { return saturate ( endTimesRcpLength - x * rcpLength ) ; } float4 Remap10 ( float4 x , float4 rcpLength , float4 endTimesRcpLength ) { return saturate ( endTimesRcpLength - x * rcpLength ) ; }
//#line 857
float2 RemapHalfTexelCoordTo01 ( float2 coord , float2 size )
{
const float2 rcpLen = size * rcp ( size - 1 ) ;
const float2 startTimesRcpLength = 0.5 * rcp ( size - 1 ) ;

return Remap01 ( coord , rcpLen , startTimesRcpLength ) ;
}
//#line 866
float2 Remap01ToHalfTexelCoord ( float2 coord , float2 size )
{
const float2 start = 0.5 * rcp ( size ) ;
const float2 len = 1 - rcp ( size ) ;

return coord * len + start ;
}
//#line 875
float Smoothstep01 ( float x )
{
return x * x * ( 3 - ( 2 * x ) ) ;
}

float Smootherstep01 ( float x )
{
return x * x * x * ( x * ( x * 6 - 15 ) + 10 ) ;
}

float Smootherstep ( float a , float b , float t )
{
float r = rcp ( b - a ) ;
float x = Remap01 ( t , r , a * r ) ;
return Smootherstep01 ( x ) ;
}

float3 NLerp ( float3 A , float3 B , float t )
{
return normalize ( lerp ( A , B , t ) ) ;
}

float Length2 ( float3 v )
{
return dot ( v , v ) ;
}
//#line 903
float Pow4 ( float x )
{
return ( x * x ) * ( x * x ) ;
}
//#line 909
float RangeRemap ( float min , float max , float t ) { return saturate ( ( t - min ) / ( max - min ) ) ; } float2 RangeRemap ( float2 min , float2 max , float2 t ) { return saturate ( ( t - min ) / ( max - min ) ) ; } float3 RangeRemap ( float3 min , float3 max , float3 t ) { return saturate ( ( t - min ) / ( max - min ) ) ; } float4 RangeRemap ( float4 min , float4 max , float4 t ) { return saturate ( ( t - min ) / ( max - min ) ) ; }
float RangeRemapFrom01 ( float min , float max , float t ) { return ( max - min ) * t + min ; } float2 RangeRemapFrom01 ( float2 min , float2 max , float2 t ) { return ( max - min ) * t + min ; } float3 RangeRemapFrom01 ( float3 min , float3 max , float3 t ) { return ( max - min ) * t + min ; } float4 RangeRemapFrom01 ( float4 min , float4 max , float4 t ) { return ( max - min ) * t + min ; }

float4x4 Inverse ( float4x4 m )
{
float n11 = m [ 0 ] [ 0 ] , n12 = m [ 1 ] [ 0 ] , n13 = m [ 2 ] [ 0 ] , n14 = m [ 3 ] [ 0 ] ;
float n21 = m [ 0 ] [ 1 ] , n22 = m [ 1 ] [ 1 ] , n23 = m [ 2 ] [ 1 ] , n24 = m [ 3 ] [ 1 ] ;
float n31 = m [ 0 ] [ 2 ] , n32 = m [ 1 ] [ 2 ] , n33 = m [ 2 ] [ 2 ] , n34 = m [ 3 ] [ 2 ] ;
float n41 = m [ 0 ] [ 3 ] , n42 = m [ 1 ] [ 3 ] , n43 = m [ 2 ] [ 3 ] , n44 = m [ 3 ] [ 3 ] ;

float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44 ;
float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44 ;
float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44 ;
float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34 ;

float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14 ;
float idet = 1.0f / det ;

float4x4 ret ;

ret [ 0 ] [ 0 ] = t11 * idet ;
ret [ 0 ] [ 1 ] = ( n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44 ) * idet ;
ret [ 0 ] [ 2 ] = ( n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44 ) * idet ;
ret [ 0 ] [ 3 ] = ( n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43 ) * idet ;

ret [ 1 ] [ 0 ] = t12 * idet ;
ret [ 1 ] [ 1 ] = ( n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44 ) * idet ;
ret [ 1 ] [ 2 ] = ( n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44 ) * idet ;
ret [ 1 ] [ 3 ] = ( n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43 ) * idet ;

ret [ 2 ] [ 0 ] = t13 * idet ;
ret [ 2 ] [ 1 ] = ( n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44 ) * idet ;
ret [ 2 ] [ 2 ] = ( n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44 ) * idet ;
ret [ 2 ] [ 3 ] = ( n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43 ) * idet ;

ret [ 3 ] [ 0 ] = t14 * idet ;
ret [ 3 ] [ 1 ] = ( n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34 ) * idet ;
ret [ 3 ] [ 2 ] = ( n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34 ) * idet ;
ret [ 3 ] [ 3 ] = ( n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33 ) * idet ;

return ret ;
}

float Remap ( float origFrom , float origTo , float targetFrom , float targetTo , float value )
{
return lerp ( targetFrom , targetTo , ( value - origFrom ) / ( origTo - origFrom ) ) ;
}
//#line 961
float ComputeTextureLOD ( float2 uvdx , float2 uvdy , float2 scale , float bias = 0.0 )
{
float2 ddx_ = scale * uvdx ;
float2 ddy_ = scale * uvdy ;
float d = max ( dot ( ddx_ , ddx_ ) , dot ( ddy_ , ddy_ ) ) ;

return max ( 0.5 * log2 ( d ) - bias , 0.0 ) ;
}

float ComputeTextureLOD ( float2 uv , float bias = 0.0 )
{
float2 ddx_ = ddx ( uv ) ;
float2 ddy_ = ddy ( uv ) ;

return ComputeTextureLOD ( ddx_ , ddy_ , 1.0 , bias ) ;
}
//#line 979
float ComputeTextureLOD ( float2 uv , float2 texelSize , float bias = 0.0 )
{
uv *= texelSize ;

return ComputeTextureLOD ( uv , bias ) ;
}
//#line 987
float ComputeTextureLOD ( float3 duvw_dx , float3 duvw_dy , float3 duvw_dz , float scale , float bias = 0.0 )
{
float d = Max3 ( dot ( duvw_dx , duvw_dx ) , dot ( duvw_dy , duvw_dy ) , dot ( duvw_dz , duvw_dz ) ) ;

return max ( 0.5f * log2 ( d * ( scale * scale ) ) - bias , 0.0 ) ;
}
//#line 1006
uint GetMipCount ( Texture2D tex , SamplerState smp )
{

uint mipLevel , width , height , mipCount ;
mipLevel = width = height = mipCount = 0 ;
tex . GetDimensions ( mipLevel , width , height , mipCount ) ;
return mipCount ;
//#line 1016
}
//#line 1111
float2 DirectionToLatLongCoordinate ( float3 unDir )
{
float3 dir = normalize ( unDir ) ;

return float2 ( 1.0 - 0.5 * 0.31830988618379067154 * atan2 ( dir . x , - dir . z ) , asin ( dir . y ) * 0.31830988618379067154 + 0.5 ) ;
}

float3 LatlongToDirectionCoordinate ( float2 coord )
{
float theta = coord . y * 3.14159265358979323846 ;
float phi = ( coord . x * 2.f * 3.14159265358979323846 - 3.14159265358979323846 * 0.5f ) ;

float cosTheta = cos ( theta ) ;
float sinTheta = sqrt ( 1.0 - min ( 1.0 , cosTheta * cosTheta ) ) ;
float cosPhi = cos ( phi ) ;
float sinPhi = sin ( phi ) ;

float3 direction = float3 ( sinTheta * cosPhi , cosTheta , sinTheta * sinPhi ) ;
direction . xy *= - 1.0 ;
return direction ;
}

float2 OrientationToDirection ( float orientation )
{
return float2 ( cos ( orientation ) , sin ( orientation ) ) ;
}
//#line 1146
float Linear01DepthFromNear ( float depth , float4 zBufferParam )
{
return 1.0 / ( zBufferParam . x + zBufferParam . y / depth ) ;
}
//#line 1155
float Linear01Depth ( float depth , float4 zBufferParam )
{
return 1.0 / ( zBufferParam . x * depth + zBufferParam . y ) ;
}
//#line 1164
float LinearEyeDepth ( float depth , float4 zBufferParam )
{
return 1.0 / ( zBufferParam . z * depth + zBufferParam . w ) ;
}
//#line 1173
float LinearEyeDepth ( float2 positionNDC , float deviceDepth , float4 invProjParam )
{
float viewSpaceZ = rcp ( dot ( float4 ( positionNDC , deviceDepth , 1.0 ) , invProjParam ) ) ;
//#line 1178
return abs ( viewSpaceZ ) ;
}
//#line 1185
float LinearEyeDepth ( float3 positionWS , float4x4 viewMatrix )
{
float viewSpaceZ = mul ( viewMatrix , float4 ( positionWS , 1.0 ) ) . z ;
//#line 1190
return abs ( viewSpaceZ ) ;
}
//#line 1200
float EncodeLogarithmicDepthGeneralized ( float z , float4 encodingParams )
{

return encodingParams . x + encodingParams . y * log2 ( max ( 0 , z - encodingParams . z ) ) ;
}
//#line 1214
float DecodeLogarithmicDepthGeneralized ( float d , float4 decodingParams )
{
return decodingParams . x * exp2 ( d * decodingParams . y ) + decodingParams . z ;
}
//#line 1223
float EncodeLogarithmicDepth ( float z , float4 encodingParams )
{
//#line 1227
return log2 ( max ( 0 , z * encodingParams . z ) ) * encodingParams . w ;
}
//#line 1235
float DecodeLogarithmicDepth ( float d , float4 encodingParams )
{

return encodingParams . x * exp2 ( d * encodingParams . y ) ;
}
//#line 1244
float EncodeInfiniteDepth ( float depth , float near )
{
return saturate ( near / depth ) ;
}
//#line 1250
float DecodeInfiniteDepth ( float z , float near )
{
return near / max ( z , 5.960464478e-8 ) ;
}

float4 CompositeOver ( float4 front , float4 back )
{
return front + ( 1 - front . a ) * back ;
}

void CompositeOver ( float3 colorFront , float3 alphaFront ,
float3 colorBack , float3 alphaBack ,
out float3 color , out float3 alpha )
{
color = colorFront + ( 1 - alphaFront ) * colorBack ;
alpha = alphaFront + ( 1 - alphaFront ) * alphaBack ;
}
//#line 1272
static const float3x3 k_identity3x3 = { 1 , 0 , 0 ,
0 , 1 , 0 ,
0 , 0 , 1 } ;

static const float4x4 k_identity4x4 = { 1 , 0 , 0 , 0 ,
0 , 1 , 0 , 0 ,
0 , 0 , 1 , 0 ,
0 , 0 , 0 , 1 } ;

float4 ComputeClipSpacePosition ( float2 positionNDC , float deviceDepth )
{
float4 positionCS = float4 ( positionNDC * 2.0 - 1.0 , deviceDepth , 1.0 ) ;
//#line 1290
positionCS . y = - positionCS . y ;
//#line 1293
return positionCS ;
}
//#line 1300
float4 ComputeClipSpacePosition ( float3 position , float4x4 clipSpaceTransform = k_identity4x4 )
{
return mul ( clipSpaceTransform , float4 ( position , 1.0 ) ) ;
}
//#line 1310
float3 ComputeNormalizedDeviceCoordinatesWithZ ( float3 position , float4x4 clipSpaceTransform = k_identity4x4 )
{
float4 positionCS = ComputeClipSpacePosition ( position , clipSpaceTransform ) ;
//#line 1319
positionCS . y = - positionCS . y ;
//#line 1322
positionCS *= rcp ( positionCS . w ) ;
positionCS . xy = positionCS . xy * 0.5 + 0.5 ;

return positionCS . xyz ;
}
//#line 1332
float2 ComputeNormalizedDeviceCoordinates ( float3 position , float4x4 clipSpaceTransform = k_identity4x4 )
{
return ComputeNormalizedDeviceCoordinatesWithZ ( position , clipSpaceTransform ) . xy ;
}

float3 ComputeViewSpacePosition ( float2 positionNDC , float deviceDepth , float4x4 invProjMatrix )
{
float4 positionCS = ComputeClipSpacePosition ( positionNDC , deviceDepth ) ;
float4 positionVS = mul ( invProjMatrix , positionCS ) ;

positionVS . z = - positionVS . z ;
return positionVS . xyz / positionVS . w ;
}

float3 ComputeWorldSpacePosition ( float2 positionNDC , float deviceDepth , float4x4 invViewProjMatrix )
{
float4 positionCS = ComputeClipSpacePosition ( positionNDC , deviceDepth ) ;
float4 hpositionWS = mul ( invViewProjMatrix , positionCS ) ;
return hpositionWS . xyz / hpositionWS . w ;
}

float3 ComputeWorldSpacePosition ( float4 positionCS , float4x4 invViewProjMatrix )
{
float4 hpositionWS = mul ( invViewProjMatrix , positionCS ) ;
return hpositionWS . xyz / hpositionWS . w ;
}
//#line 1364
struct PositionInputs
{
float3 positionWS ;
float2 positionNDC ;
uint2 positionSS ;
uint2 tileCoord ;
float deviceDepth ;
float linearDepth ;
} ;
//#line 1378
PositionInputs GetPositionInput ( float2 positionSS , float2 invScreenSize , uint2 tileCoord )
{
PositionInputs posInput ;
posInput = ( PositionInputs ) 0 ; ;

posInput . positionNDC = positionSS ;
//#line 1388
posInput . positionNDC *= invScreenSize ;
posInput . positionSS = uint2 ( positionSS ) ;
posInput . tileCoord = tileCoord ;

return posInput ;
}

PositionInputs GetPositionInput ( float2 positionSS , float2 invScreenSize )
{
return GetPositionInput ( positionSS , invScreenSize , uint2 ( 0 , 0 ) ) ;
}
//#line 1402
PositionInputs GetPositionInput ( float2 positionSS , float2 invScreenSize , float3 positionWS )
{
PositionInputs posInput = GetPositionInput ( positionSS , invScreenSize , uint2 ( 0 , 0 ) ) ;
posInput . positionWS = positionWS ;

return posInput ;
}
//#line 1412
PositionInputs GetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth , float linearDepth , float3 positionWS , uint2 tileCoord )
{
PositionInputs posInput = GetPositionInput ( positionSS , invScreenSize , tileCoord ) ;
posInput . positionWS = positionWS ;
posInput . deviceDepth = deviceDepth ;
posInput . linearDepth = linearDepth ;

return posInput ;
}

PositionInputs GetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth , float linearDepth , float3 positionWS )
{
return GetPositionInput ( positionSS , invScreenSize , deviceDepth , linearDepth , positionWS , uint2 ( 0 , 0 ) ) ;
}
//#line 1430
PositionInputs GetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth ,
float4x4 invViewProjMatrix , float4x4 viewMatrix ,
uint2 tileCoord )
{
PositionInputs posInput = GetPositionInput ( positionSS , invScreenSize , tileCoord ) ;
posInput . positionWS = ComputeWorldSpacePosition ( posInput . positionNDC , deviceDepth , invViewProjMatrix ) ;
posInput . deviceDepth = deviceDepth ;
posInput . linearDepth = LinearEyeDepth ( posInput . positionWS , viewMatrix ) ;

return posInput ;
}

PositionInputs GetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth ,
float4x4 invViewProjMatrix , float4x4 viewMatrix )
{
return GetPositionInput ( positionSS , invScreenSize , deviceDepth , invViewProjMatrix , viewMatrix , uint2 ( 0 , 0 ) ) ;
}
//#line 1450
void ApplyDepthOffsetPositionInput ( float3 V , float depthOffsetVS , float3 viewForwardDir , float4x4 viewProjMatrix , inout PositionInputs posInput )
{
posInput . positionWS += depthOffsetVS * ( - V ) ;
posInput . deviceDepth = ComputeNormalizedDeviceCoordinatesWithZ ( posInput . positionWS , viewProjMatrix ) . z ;
//#line 1458
posInput . linearDepth += depthOffsetVS * abs ( dot ( V , viewForwardDir ) ) ;
}
//#line 1485
float4 PackHeightmap ( float height )
{
return float4 ( height , 0 , 0 , 0 ) ;
}

float UnpackHeightmap ( float4 height )
{
return height . r ;
}
//#line 1503
bool HasFlag ( uint bitfield , uint flag )
{
return ( bitfield & flag ) != 0 ;
}
//#line 1509
float3 SafeNormalize ( float3 inVec )
{
float dp3 = max ( 1.175494351e-38 , dot ( inVec , inVec ) ) ;
return inVec * rsqrt ( dp3 ) ;
}

half3 SafeNormalize ( half3 inVec )
{
half dp3 = max ( 6.103515625e-5 , dot ( inVec , inVec ) ) ;
return inVec * rsqrt ( dp3 ) ;
}

bool IsNormalized ( float3 inVec )
{
float squaredLength = dot ( inVec , inVec ) ;
return 0.9998 < squaredLength && squaredLength < 1.0002001 ;
}

bool IsNormalized ( half3 inVec )
{
half squaredLength = dot ( inVec , inVec ) ;
return 0.998 < squaredLength && squaredLength < 1.002 ;
}
//#line 1535
float SafeDiv ( float numer , float denom )
{
return ( numer != denom ) ? numer / denom : 1 ;
}
//#line 1541
float SafeSqrt ( float x )
{
return sqrt ( max ( 0 , x ) ) ;
}
//#line 1547
float SinFromCos ( float cosX )
{
return sqrt ( saturate ( 1 - cosX * cosX ) ) ;
}
//#line 1553
float SphericalDot ( float cosTheta1 , float phi1 , float cosTheta2 , float phi2 )
{
return SinFromCos ( cosTheta1 ) * SinFromCos ( cosTheta2 ) * cos ( phi1 - phi2 ) + cosTheta1 * cosTheta2 ;
}
//#line 1560
float2 GetFullScreenTriangleTexCoord ( uint vertexID )
{

return float2 ( ( vertexID << 1 ) & 2 , 1.0 - ( vertexID & 2 ) ) ;
//#line 1567
}

float4 GetFullScreenTriangleVertexPosition ( uint vertexID , float z = ( 1.0 ) )
{

float2 uv = float2 ( ( vertexID << 1 ) & 2 , vertexID & 2 ) ;
float4 pos = float4 ( uv * 2.0 - 1.0 , z , 1.0 ) ;
//#line 1577
return pos ;
}
//#line 1588
float2 GetQuadTexCoord ( uint vertexID )
{
uint topBit = vertexID >> 1 ;
uint botBit = ( vertexID & 1 ) ;
float u = topBit ;
float v = ( topBit + botBit ) & 1 ;

v = 1.0 - v ;

return float2 ( u , v ) ;
}
//#line 1604
float4 GetQuadVertexPosition ( uint vertexID , float z = ( 1.0 ) )
{
uint topBit = vertexID >> 1 ;
uint botBit = ( vertexID & 1 ) ;
float x = topBit ;
float y = 1 - ( topBit + botBit ) & 1 ;
float4 pos = float4 ( x , y , z , 1.0 ) ;
//#line 1614
return pos ;
}
//#line 1623
void LODDitheringTransition ( uint2 fadeMaskSeed , float ditherFactor )
{
//#line 1627
float p = GenerateHashedRandomFloat ( fadeMaskSeed ) ;
//#line 1630
float f = ditherFactor - CopySign ( p , ditherFactor ) ;
clip ( f ) ;
}
//#line 1638
uint GetStencilValue ( uint2 stencilBufferVal )
{

return stencilBufferVal . y ;
//#line 1645
}
//#line 1650
float SharpenAlpha ( float alpha , float alphaClipTreshold )
{
return saturate ( ( alpha - alphaClipTreshold ) / max ( fwidth ( alpha ) , 0.0001 ) + 0.5 ) ;
}
//#line 1656
float ClampToFloat16Max ( float value ) { return min ( value , 65504.0 ) ; } float2 ClampToFloat16Max ( float2 value ) { return min ( value , 65504.0 ) ; } float3 ClampToFloat16Max ( float3 value ) { return min ( value , 65504.0 ) ; } float4 ClampToFloat16Max ( float4 value ) { return min ( value , 65504.0 ) ; }
//#line 1662
float2 RepeatOctahedralUV ( float u , float v )
{
float2 uv ;

if ( u < 0.0f )
{
if ( v < 0.0f )
uv = float2 ( 1.0f + u , 1.0f + v ) ;
else if ( v < 1.0f )
uv = float2 ( - u , 1.0f - v ) ;
else
uv = float2 ( 1.0f + u , v - 1.0f ) ;
}
else if ( u < 1.0f )
{
if ( v < 0.0f )
uv = float2 ( 1.0f - u , - v ) ;
else if ( v < 1.0f )
uv = float2 ( u , v ) ;
else
uv = float2 ( 1.0f - u , 2.0f - v ) ;
}
else
{
if ( v < 0.0f )
uv = float2 ( u - 1.0f , 1.0f + v ) ;
else if ( v < 1.0f )
uv = float2 ( 2.0f - u , 1.0f - v ) ;
else
uv = float2 ( u - 1.0f , v - 1.0f ) ;
}

return uv ;
}
//#line 12 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
float3 PackNormalMaxComponent ( float3 n )
{
return ( n / Max3 ( abs ( n . x ) , abs ( n . y ) , abs ( n . z ) ) ) * 0.5 + 0.5 ;
}

float3 UnpackNormalMaxComponent ( float3 n )
{
return normalize ( n * 2.0 - 1.0 ) ;
}
//#line 25
float2 PackNormalOctRectEncode ( float3 n )
{

float3 p = n * rcp ( dot ( abs ( n ) , 1.0 ) ) ;
float x = p . x , y = p . y , z = p . z ;
//#line 33
float r = saturate ( 0.5 - 0.5 * x + 0.5 * y ) ;
float g = x + y ;
//#line 37
return float2 ( CopySign ( r , z ) , g ) ;
}

float3 UnpackNormalOctRectEncode ( float2 f )
{
float r = f . r , g = f . g ;
//#line 45
float x = 0.5 + 0.5 * g - abs ( r ) ;
float y = g - x ;
float z = max ( 1.0 - abs ( x ) - abs ( y ) , 5.960464478e-8 ) ;

float3 p = float3 ( x , y , CopySign ( z , r ) ) ;

return normalize ( p ) ;
}
//#line 57
float2 PackNormalOctQuadEncode ( float3 n )
{
//#line 66
n *= rcp ( max ( dot ( abs ( n ) , 1.0 ) , 1e-6 ) ) ;
float t = saturate ( - n . z ) ;
return n . xy + float2 ( n . x >= 0.0 ? t : - t , n . y >= 0.0 ? t : - t ) ;
}

float3 UnpackNormalOctQuadEncode ( float2 f )
{
float3 n = float3 ( f . x , f . y , 1.0 - abs ( f . x ) - abs ( f . y ) ) ;
//#line 79
float t = max ( - n . z , 0.0 ) ;
n . xy += float2 ( n . x >= 0.0 ? - t : t , n . y >= 0.0 ? - t : t ) ;

return normalize ( n ) ;
}

float2 PackNormalHemiOctEncode ( float3 n )
{
float l1norm = dot ( abs ( n ) , 1.0 ) ;
float2 res = n . xy * ( 1.0 / l1norm ) ;

return float2 ( res . x + res . y , res . x - res . y ) ;
}

float3 UnpackNormalHemiOctEncode ( float2 f )
{
float2 val = float2 ( f . x + f . y , f . x - f . y ) * 0.5 ;
float3 n = float3 ( val , 1.0 - dot ( abs ( val ) , 1.0 ) ) ;

return normalize ( n ) ;
}
//#line 109
static const float3 tetraBasisNormal [ 4 ] =
{
float3 ( 0. , 0.816497 , - 0.57735 ) ,
float3 ( - 0.816497 , 0. , 0.57735 ) ,
float3 ( 0.816497 , 0. , 0.57735 ) ,
float3 ( 0. , - 0.816497 , - 0.57735 )
} ;
//#line 118
static const float3x3 tetraBasisArray [ 4 ] =
{
float3x3 ( - 1. , 0. , 0. , 0. , 0.57735 , 0.816497 , 0. , 0.816497 , - 0.57735 ) ,
float3x3 ( 0. , - 1. , 0. , 0.57735 , 0. , 0.816497 , - 0.816497 , 0. , 0.57735 ) ,
float3x3 ( 0. , 1. , 0. , - 0.57735 , 0. , 0.816497 , 0.816497 , 0. , 0.57735 ) ,
float3x3 ( 1. , 0. , 0. , 0. , - 0.57735 , 0.816497 , 0. , - 0.816497 , - 0.57735 )
} ;
//#line 127
float2 PackNormalTetraEncode ( float3 n , out uint faceIndex )
{
//#line 131
float dot0 = dot ( n , tetraBasisNormal [ 0 ] ) ;
float dot1 = dot ( n , tetraBasisNormal [ 1 ] ) ;
float dot2 = dot ( n , tetraBasisNormal [ 2 ] ) ;
float dot3 = dot ( n , tetraBasisNormal [ 3 ] ) ;

float maxi0 = max ( dot0 , dot1 ) ;
float maxi1 = max ( dot2 , dot3 ) ;
float maxi = max ( maxi0 , maxi1 ) ;
//#line 141
if ( maxi == dot0 )
faceIndex = 0 ;
else if ( maxi == dot1 )
faceIndex = 1 ;
else if ( maxi == dot2 )
faceIndex = 2 ;
else
faceIndex = 3 ;
//#line 151
n = mul ( tetraBasisArray [ faceIndex ] , n ) ;
//#line 154
return n . xy ;
}
//#line 158
float3 UnpackNormalTetraEncode ( float2 f , uint faceIndex )
{

float3 n = float3 ( f . xy , sqrt ( 1.0 - dot ( f . xy , f . xy ) ) ) ;

return mul ( n , tetraBasisArray [ faceIndex ] ) ;
}
//#line 167
float3 UnpackNormalRGB ( float4 packedNormal , float scale = 1.0 )
{
float3 normal ;
normal . xyz = packedNormal . rgb * 2.0 - 1.0 ;
normal . xy *= scale ;
return normal ;
}

float3 UnpackNormalRGBNoScale ( float4 packedNormal )
{
return packedNormal . rgb * 2.0 - 1.0 ;
}

float3 UnpackNormalAG ( float4 packedNormal , float scale = 1.0 )
{
float3 normal ;
normal . xy = packedNormal . ag * 2.0 - 1.0 ;
normal . z = max ( 1.0e-16 , sqrt ( 1.0 - saturate ( dot ( normal . xy , normal . xy ) ) ) ) ;
//#line 193
normal . xy *= scale ;
return normal ;
}
//#line 198
float3 UnpackNormalmapRGorAG ( float4 packedNormal , float scale = 1.0 )
{

packedNormal . a *= packedNormal . r ;
return UnpackNormalAG ( packedNormal , scale ) ;
}
//#line 206
float3 UnpackNormal ( float4 packedNormal )
{
//#line 214
return UnpackNormalmapRGorAG ( packedNormal , 1.0 ) ;

}
//#line 219
float3 UnpackNormalScale ( float4 packedNormal , float bumpScale )
{
//#line 226
return UnpackNormalmapRGorAG ( packedNormal , bumpScale ) ;

}
//#line 235
float4 PackToLogLuv ( float3 vRGB )
{

const float3x3 M = float3x3 (
0.2209 , 0.3390 , 0.4184 ,
0.1138 , 0.6780 , 0.7319 ,
0.0102 , 0.1130 , 0.2969 ) ;

float4 vResult ;
float3 Xp_Y_XYZp = mul ( vRGB , M ) ;
Xp_Y_XYZp = max ( Xp_Y_XYZp , float3 ( 1e-6 , 1e-6 , 1e-6 ) ) ;
vResult . xy = Xp_Y_XYZp . xy / Xp_Y_XYZp . z ;
float Le = 2.0 * log2 ( Xp_Y_XYZp . y ) + 127.0 ;
vResult . w = frac ( Le ) ;
vResult . z = ( Le - ( floor ( vResult . w * 255.0 ) ) / 255.0 ) / 255.0 ;
return vResult ;
}

float3 UnpackFromLogLuv ( float4 vLogLuv )
{

const float3x3 InverseM = float3x3 (
6.0014 , - 2.7008 , - 1.7996 ,
- 1.3320 , 3.1029 , - 5.7721 ,
0.3008 , - 1.0882 , 5.6268 ) ;

float Le = vLogLuv . z * 255.0 + vLogLuv . w ;
float3 Xp_Y_XYZp ;
Xp_Y_XYZp . y = exp2 ( ( Le - 127.0 ) / 2.0 ) ;
Xp_Y_XYZp . z = Xp_Y_XYZp . y / vLogLuv . y ;
Xp_Y_XYZp . x = vLogLuv . x * Xp_Y_XYZp . z ;
float3 vRGB = mul ( Xp_Y_XYZp , InverseM ) ;
return max ( vRGB , float3 ( 0.0 , 0.0 , 0.0 ) ) ;
}
//#line 271
uint PackToR11G11B10f ( float3 rgb )
{
uint r = ( f32tof16 ( rgb . x ) << 17 ) & 0xFFE00000 ;
uint g = ( f32tof16 ( rgb . y ) << 6 ) & 0x001FFC00 ;
uint b = ( f32tof16 ( rgb . z ) >> 5 ) & 0x000003FF ;
return r | g | b ;
}

float3 UnpackFromR11G11B10f ( uint rgb )
{
float r = f16tof32 ( ( rgb >> 17 ) & 0x7FF0 ) ;
float g = f16tof32 ( ( rgb >> 6 ) & 0x7FF0 ) ;
float b = f16tof32 ( ( rgb << 5 ) & 0x7FE0 ) ;
return float3 ( r , g , b ) ;
}
//#line 323
float4 UnpackQuat ( float4 packedQuat )
{
uint index = ( uint ) ( packedQuat . w * 3.0 ) ;

float4 quat ;
quat . xyz = packedQuat . xyz * sqrt ( 2.0 ) - ( 1.0 / sqrt ( 2.0 ) ) ;
quat . w = sqrt ( 1.0 - saturate ( dot ( quat . xyz , quat . xyz ) ) ) ;

if ( index == 0 ) quat = quat . wxyz ;
if ( index == 1 ) quat = quat . xwyz ;
if ( index == 2 ) quat = quat . xywz ;

return quat ;
}
//#line 343
float PackInt ( uint i , uint numBits )
{
uint maxInt = ( 1u << numBits ) - 1u ;
return saturate ( i * rcp ( maxInt ) ) ;
}
//#line 350
uint UnpackInt ( float f , uint numBits )
{
uint maxInt = ( 1u << numBits ) - 1u ;
return ( uint ) ( f * maxInt + 0.5 ) ;
}
//#line 357
float PackByte ( uint i )
{
return PackInt ( i , 8 ) ;
}
//#line 363
uint UnpackByte ( float f )
{
return UnpackInt ( f , 8 ) ;
}
//#line 369
float PackShort ( uint i )
{
return PackInt ( i , 16 ) ;
}
//#line 375
uint UnpackShort ( float f )
{
return UnpackInt ( f , 16 ) ;
}
//#line 381
float PackShortLo ( uint i )
{
uint lo = BitFieldExtract ( i , 0u , 8u ) ;
return PackInt ( lo , 8 ) ;
}
//#line 388
float PackShortHi ( uint i )
{
uint hi = BitFieldExtract ( i , 8u , 8u ) ;
return PackInt ( hi , 8 ) ;
}

float Pack2Byte ( float2 inputs )
{
float2 temp = inputs * float2 ( 255.0 , 255.0 ) ;
temp . x *= 256.0 ;
temp = round ( temp ) ;
float combined = temp . x + temp . y ;
return combined * ( 1.0 / 65535.0 ) ;
}

float2 Unpack2Byte ( float inputs )
{
float temp = round ( inputs * 65535.0 ) ;
float ipart ;
float fpart = modf ( temp / 256.0 , ipart ) ;
float2 result = float2 ( ipart , round ( 256.0 * fpart ) ) ;
return result * ( 1.0 / float2 ( 255.0 , 255.0 ) ) ;
}
//#line 420
float PackFloatInt ( float f , uint i , float maxi , float precision )
{

float precisionMinusOne = precision - 1.0 ;
float t1 = ( ( precision / maxi ) - 1.0 ) / precisionMinusOne ;
float t2 = ( precision / maxi ) / precisionMinusOne ;

return t1 * f + t2 * float ( i ) ;
}

void UnpackFloatInt ( float val , float maxi , float precision , out float f , out uint i )
{

float precisionMinusOne = precision - 1.0 ;
float t1 = ( ( precision / maxi ) - 1.0 ) / precisionMinusOne ;
float t2 = ( precision / maxi ) / precisionMinusOne ;
//#line 438
i = int ( ( val / t2 ) + rcp ( precisionMinusOne ) ) ;
//#line 441
f = saturate ( ( - t2 * float ( i ) + val ) / t1 ) ;
}
//#line 445
float PackFloatInt8bit ( float f , uint i , float maxi )
{
return PackFloatInt ( f , i , maxi , 256.0 ) ;
}

void UnpackFloatInt8bit ( float val , float maxi , out float f , out uint i )
{
UnpackFloatInt ( val , maxi , 256.0 , f , i ) ;
}

float PackFloatInt10bit ( float f , uint i , float maxi )
{
return PackFloatInt ( f , i , maxi , 1024.0 ) ;
}

void UnpackFloatInt10bit ( float val , float maxi , out float f , out uint i )
{
UnpackFloatInt ( val , maxi , 1024.0 , f , i ) ;
}

float PackFloatInt16bit ( float f , uint i , float maxi )
{
return PackFloatInt ( f , i , maxi , 65536.0 ) ;
}

void UnpackFloatInt16bit ( float val , float maxi , out float f , out uint i )
{
UnpackFloatInt ( val , maxi , 65536.0 , f , i ) ;
}
//#line 480
uint PackFloatToUInt ( float src , uint offset , uint numBits )
{
return UnpackInt ( src , numBits ) << offset ;
}

float UnpackUIntToFloat ( uint src , uint offset , uint numBits )
{
uint maxInt = ( 1u << numBits ) - 1u ;
return float ( BitFieldExtract ( src , offset , numBits ) ) * rcp ( maxInt ) ;
}

uint PackToR10G10B10A2 ( float4 rgba )
{
return ( PackFloatToUInt ( rgba . x , 0 , 10 ) |
PackFloatToUInt ( rgba . y , 10 , 10 ) |
PackFloatToUInt ( rgba . z , 20 , 10 ) |
PackFloatToUInt ( rgba . w , 30 , 2 ) ) ;
}

float4 UnpackFromR10G10B10A2 ( uint rgba )
{
float4 output ;
output . x = UnpackUIntToFloat ( rgba , 0 , 10 ) ;
output . y = UnpackUIntToFloat ( rgba , 10 , 10 ) ;
output . z = UnpackUIntToFloat ( rgba , 20 , 10 ) ;
output . w = UnpackUIntToFloat ( rgba , 30 , 2 ) ;
return output ;
}
//#line 510
float2 PackFloatToR8G8 ( float f )
{
uint i = UnpackShort ( f ) ;
return float2 ( PackShortLo ( i ) , PackShortHi ( i ) ) ;
}
//#line 517
float UnpackFloatFromR8G8 ( float2 f )
{
uint lo = UnpackByte ( f . x ) ;
uint hi = UnpackByte ( f . y ) ;
uint cb = ( hi << 8 ) + lo ;
return PackShort ( cb ) ;
}
//#line 526
uint3 PackFloat2To888UInt ( float2 f )
{
uint2 i = ( uint2 ) ( f * 4095.5 ) ;
uint2 hi = i >> 8 ;
uint2 lo = i & 255 ;

uint3 cb = uint3 ( lo , hi . x | ( hi . y << 4 ) ) ;
return cb ;
}
//#line 537
float3 PackFloat2To888 ( float2 f )
{
return PackFloat2To888UInt ( f ) / 255.0 ;
}
//#line 543
float2 Unpack888UIntToFloat2 ( uint3 x )
{

uint hi = x . z >> 4 ;
uint lo = x . z & 15 ;
uint2 cb = x . xy | uint2 ( lo << 8 , hi << 8 ) ;

return cb / 4095.0 ;
}
//#line 554
float2 Unpack888ToFloat2 ( float3 x )
{
uint3 i = ( uint3 ) ( x * 255.5 ) ;
return Unpack888UIntToFloat2 ( i ) ;
}
//#line 561
float PackFloat2To8 ( float2 f )
{
float2 i = floor ( f * 15.0 ) ;
float x_y_expanded = i . x * 16.0 + i . y ;
return x_y_expanded / 255.0 ;
//#line 569
}
//#line 572
float2 Unpack8ToFloat2 ( float f )
{
float x_y_expanded = 255.0 * f ;
float x_expanded = floor ( x_y_expanded / 16.0 ) ;
float y_expanded = x_y_expanded - 16.0 * x_expanded ;
float x = x_expanded / 15.0 ;
float y = y_expanded / 15.0 ;
return float2 ( x , y ) ;
}
//#line 586
float4 UnpackFromR8G8B8A8 ( uint rgba )
{
return float4 ( rgba & 255 , ( rgba >> 8 ) & 255 , ( rgba >> 16 ) & 255 , ( rgba >> 24 ) & 255 ) * ( 1.0 / 255 ) ;
}

float2 PackToR5G6B5 ( float3 rgb )
{
uint rgb16 = ( PackFloatToUInt ( rgb . x , 0 , 5 ) |
PackFloatToUInt ( rgb . y , 5 , 6 ) |
PackFloatToUInt ( rgb . z , 11 , 5 ) ) ;
return float2 ( PackByte ( rgb16 >> 8 ) , PackByte ( rgb16 & 0xFF ) ) ;
}

float3 UnpackFromR5G6B5 ( float2 rgb )
{
uint rgb16 = ( UnpackByte ( rgb . x ) << 8 ) | UnpackByte ( rgb . y ) ;
return float3 ( UnpackUIntToFloat ( rgb16 , 0 , 5 ) ,
UnpackUIntToFloat ( rgb16 , 5 , 6 ) ,
UnpackUIntToFloat ( rgb16 , 11 , 5 ) ) ;
}

uint PackToR7G7B6 ( float3 rgb )
{
uint rgb20 = ( PackFloatToUInt ( rgb . x , 0 , 7 ) |
PackFloatToUInt ( rgb . y , 7 , 7 ) |
PackFloatToUInt ( rgb . z , 14 , 6 ) ) ;
return rgb20 ;
}

float3 UnpackFromR7G7B6 ( uint rgb )
{
return float3 ( UnpackUIntToFloat ( rgb , 0 , 7 ) ,
UnpackUIntToFloat ( rgb , 7 , 7 ) ,
UnpackUIntToFloat ( rgb , 14 , 6 ) ) ;
}
//#line 7 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
SamplerState sampler_PointClamp ;
SamplerState sampler_LinearClamp ;
SamplerState sampler_TrilinearClamp ;
SamplerState sampler_PointRepeat ;
SamplerState sampler_LinearRepeat ;
SamplerState sampler_TrilinearRepeat ;
//#line 9 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/ShaderTypes.cs.hlsl"
struct LightData
{
float4 position ;
float4 color ;
float4 attenuation ;
float4 spotDirection ;
float4 occlusionProbeChannels ;
uint layerMask ;
} ;
//#line 43 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
struct InputData
{
float3 positionWS ;
float4 positionCS ;
float3 normalWS ;
half3 viewDirectionWS ;
float4 shadowCoord ;
half fogCoord ;
half3 vertexLighting ;
half3 bakedGI ;
float2 normalizedScreenSpaceUV ;
half4 shadowMask ;
half3x3 tangentToWorld ;
//#line 92
} ;
//#line 98
half4 _GlossyEnvironmentColor ;
half4 _SubtractiveShadowColor ;

half4 _GlossyEnvironmentCubeMap_HDR ;
TextureCube _GlossyEnvironmentCubeMap ;
SamplerState sampler_GlossyEnvironmentCubeMap ;
//#line 106
float4 _ScaledScreenParams ;
//#line 110
float2 _GlobalMipBias ;
//#line 113
float _AlphaToMaskAvailable ;

float4 _MainLightPosition ;

half4 _MainLightColor ;
half4 _MainLightOcclusionProbes ;
uint _MainLightLayerMask ;
//#line 124
half4 _AmbientOcclusionParam ;

half4 _AdditionalLightsCount ;

uint _RenderingLayerMaxInt ;
float _RenderingLayerRcpMaxInt ;
//#line 132
float4 _ScreenCoordScaleBias ;
float4 _ScreenSizeOverride ;

uint _EnableProbeVolumes ;
//#line 138
float4 _FPParams0 ;
float4 _FPParams1 ;
float4 _FPParams2 ;
//#line 165
cbuffer AdditionalLights {

float4 _AdditionalLightsPosition [ ( 256 ) ] ;

half4 _AdditionalLightsColor [ ( 256 ) ] ;
half4 _AdditionalLightsAttenuation [ ( 256 ) ] ;
half4 _AdditionalLightsSpotDir [ ( 256 ) ] ;
half4 _AdditionalLightsOcclusionProbes [ ( 256 ) ] ;
float _AdditionalLightsLayerMasks [ ( 256 ) ] ;

} ;
//#line 181
cbuffer urp_ZBinBuffer {
float4 urp_ZBins [ 1024 ] ;
} ;
cbuffer urp_TileBuffer {
float4 urp_Tiles [ 4096 ] ;
} ;

Texture2D urp_ReflProbes_Atlas ;
float urp_ReflProbes_Count ;
//#line 195
cbuffer urp_ReflectionProbeBuffer {

float4 urp_ReflProbes_BoxMax [ 64 ] ;
float4 urp_ReflProbes_BoxMin [ 64 ] ;
float4 urp_ReflProbes_ProbePosition [ 64 ] ;
float4 urp_ReflProbes_MipScaleOffset [ 64 * 7 ] ;

} ;
//#line 42 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
float4 _Time ;
float4 _SinTime ;
float4 _CosTime ;
float4 unity_DeltaTime ;
float4 _TimeParameters ;
float4 _LastTimeParameters ;
//#line 50
float3 _WorldSpaceCameraPos ;
//#line 57
float4 _ProjectionParams ;
//#line 63
float4 _ScreenParams ;
//#line 75
float4 _ZBufferParams ;
//#line 81
float4 unity_OrthoParams ;
//#line 87
uniform float4 _ScaleBias ;
uniform float4 _ScaleBiasRt ;
//#line 91
uniform float4 _RTHandleScale ;

float4 unity_CameraWorldClipPlanes [ 6 ] ;
//#line 99
float4x4 unity_CameraProjection ;
float4x4 unity_CameraInvProjection ;
float4x4 unity_WorldToCamera ;
float4x4 unity_CameraToWorld ;
//#line 110
cbuffer UnityPerDraw {

float4x4 unity_ObjectToWorld ;
float4x4 unity_WorldToObject ;
float4 unity_LODFade ;
float4 unity_WorldTransformParams ;
//#line 119
float4 unity_RenderingLayer ;
//#line 123
half4 unity_LightData ;
half4 unity_LightIndices [ 2 ] ;

float4 unity_ProbesOcclusion ;
//#line 130
float4 unity_SpecCube0_HDR ;
float4 unity_SpecCube1_HDR ;

float4 unity_SpecCube0_BoxMax ;
float4 unity_SpecCube0_BoxMin ;
float4 unity_SpecCube0_ProbePosition ;
float4 unity_SpecCube1_BoxMax ;
float4 unity_SpecCube1_BoxMin ;
float4 unity_SpecCube1_ProbePosition ;
//#line 141
float4 unity_LightmapST ;
float4 unity_DynamicLightmapST ;
//#line 145
float4 unity_SHAr ;
float4 unity_SHAg ;
float4 unity_SHAb ;
float4 unity_SHBr ;
float4 unity_SHBg ;
float4 unity_SHBb ;
float4 unity_SHC ;
//#line 154
float4 unity_RendererBounds_Min ;
float4 unity_RendererBounds_Max ;
//#line 158
float4x4 unity_MatrixPreviousM ;
float4x4 unity_MatrixPreviousMI ;
//#line 164
float4 unity_MotionVectorsParams ;
//#line 167
float4 unity_SpriteColor ;
//#line 172
float4 unity_SpriteProps ;
} ;
//#line 209
float4x4 glstate_matrix_transpose_modelview0 ;
//#line 213
float4 glstate_lightmodel_ambient ;
float4 unity_AmbientSky ;
float4 unity_AmbientEquator ;
float4 unity_AmbientGround ;
float4 unity_IndirectSpecColor ;
float4 unity_FogParams ;
float4 unity_FogColor ;
//#line 222
float4x4 glstate_matrix_projection ;
float4x4 unity_MatrixV ;
float4x4 unity_MatrixInvV ;
float4x4 unity_MatrixInvP ;
float4x4 unity_MatrixVP ;
float4x4 unity_MatrixInvVP ;
float4 unity_StereoScaleOffset ;
int unity_StereoEyeIndex ;
//#line 232
float4 unity_ShadowColor ;
//#line 237
TextureCube unity_SpecCube0 ;
SamplerState samplerunity_SpecCube0 ;
TextureCube unity_SpecCube1 ;
SamplerState samplerunity_SpecCube1 ;
//#line 243
Texture2D unity_Lightmap ;
SamplerState samplerunity_Lightmap ;
Texture2DArray unity_Lightmaps ;
SamplerState samplerunity_Lightmaps ;
//#line 249
Texture2D unity_DynamicLightmap ;
SamplerState samplerunity_DynamicLightmap ;
//#line 254
Texture2D unity_LightmapInd ;
Texture2DArray unity_LightmapsInd ;
Texture2D unity_DynamicDirectionality ;
//#line 260
Texture2D unity_ShadowMask ;
SamplerState samplerunity_ShadowMask ;
Texture2DArray unity_ShadowMasks ;
SamplerState samplerunity_ShadowMasks ;
//#line 266
Texture2D unity_MipmapStreaming_DebugTex ;
//#line 281
float4x4 _PrevViewProjMatrix ;
float4x4 _NonJitteredViewProjMatrix ;
float4x4 _ViewProjMatrix ;

float4x4 _ViewMatrix ;
float4x4 _ProjMatrix ;
float4x4 _InvViewProjMatrix ;
float4x4 _InvViewMatrix ;
float4x4 _InvProjMatrix ;
float4 _InvProjParam ;
float4 _ScreenSize ;
float4 _FrustumPlanes [ 6 ] ;

float4x4 OptimizeProjectionMatrix ( float4x4 M )
{
//#line 304
M . _21_41 = 0 ;
M . _12_42 = 0 ;
return M ;
}
//#line 11 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
float4x4 GetObjectToWorldMatrix ( )
{
return unity_ObjectToWorld ;
}

float4x4 GetWorldToObjectMatrix ( )
{
return unity_WorldToObject ;
}

float4x4 GetPrevObjectToWorldMatrix ( )
{
return unity_MatrixPreviousM ;
}

float4x4 GetPrevWorldToObjectMatrix ( )
{
return unity_MatrixPreviousMI ;
}

float4x4 GetWorldToViewMatrix ( )
{
return unity_MatrixV ;
}

float4x4 GetViewToWorldMatrix ( )
{
return unity_MatrixInvV ;
}
//#line 42
float4x4 GetWorldToHClipMatrix ( )
{
return unity_MatrixVP ;
}
//#line 48
float4x4 GetViewToHClipMatrix ( )
{
return OptimizeProjectionMatrix ( glstate_matrix_projection ) ;
}
//#line 54
float3 GetAbsolutePositionWS ( float3 positionRWS )
{
//#line 59
return positionRWS ;
}
//#line 63
float3 GetCameraRelativePositionWS ( float3 positionWS )
{
//#line 68
return positionWS ;
}

float GetOddNegativeScale ( )
{
//#line 76
return unity_WorldTransformParams . w >= 0.0 ? 1.0 : - 1.0 ;
}

float3 TransformObjectToWorld ( float3 positionOS )
{
//#line 84
return mul ( GetObjectToWorldMatrix ( ) , float4 ( positionOS , 1.0 ) ) . xyz ;

}

float3 TransformWorldToObject ( float3 positionWS )
{
//#line 93
return mul ( GetWorldToObjectMatrix ( ) , float4 ( positionWS , 1.0 ) ) . xyz ;

}

float3 TransformWorldToView ( float3 positionWS )
{
return mul ( GetWorldToViewMatrix ( ) , float4 ( positionWS , 1.0 ) ) . xyz ;
}

float3 TransformViewToWorld ( float3 positionVS )
{
return mul ( GetViewToWorldMatrix ( ) , float4 ( positionVS , 1.0 ) ) . xyz ;
}
//#line 108
float4 TransformObjectToHClip ( float3 positionOS )
{

return mul ( GetWorldToHClipMatrix ( ) , mul ( GetObjectToWorldMatrix ( ) , float4 ( positionOS , 1.0 ) ) ) ;
}
//#line 115
float4 TransformWorldToHClip ( float3 positionWS )
{
return mul ( GetWorldToHClipMatrix ( ) , float4 ( positionWS , 1.0 ) ) ;
}
//#line 121
float4 TransformWViewToHClip ( float3 positionVS )
{
return mul ( GetViewToHClipMatrix ( ) , float4 ( positionVS , 1.0 ) ) ;
}
//#line 127
float3 TransformObjectToWorldDir ( float3 dirOS , bool doNormalize = true )
{

float3 dirWS = mul ( ( float3x3 ) GetObjectToWorldMatrix ( ) , dirOS ) ;
//#line 134
if ( doNormalize )
return SafeNormalize ( dirWS ) ;

return dirWS ;
}
//#line 141
float3 TransformWorldToObjectDir ( float3 dirWS , bool doNormalize = true )
{

float3 dirOS = mul ( ( float3x3 ) GetWorldToObjectMatrix ( ) , dirWS ) ;
//#line 148
if ( doNormalize )
return normalize ( dirOS ) ;

return dirOS ;
}
//#line 155
float3 TransformWorldToViewDir ( float3 dirWS , bool doNormalize = false )
{
float3 dirVS = mul ( ( float3x3 ) GetWorldToViewMatrix ( ) , dirWS ) . xyz ;
if ( doNormalize )
return normalize ( dirVS ) ;

return dirVS ;
}
//#line 165
float3 TransformViewToWorldDir ( float3 dirVS , bool doNormalize = false )
{
float3 dirWS = mul ( ( float3x3 ) GetViewToWorldMatrix ( ) , dirVS ) . xyz ;
if ( doNormalize )
return normalize ( dirWS ) ;

return dirWS ;
}
//#line 175
float3 TransformWorldToViewNormal ( float3 normalWS , bool doNormalize = false )
{

return TransformWorldToViewDir ( normalWS , doNormalize ) ;
}
//#line 182
float3 TransformViewToWorldNormal ( float3 normalVS , bool doNormalize = false )
{

return TransformViewToWorldDir ( normalVS , doNormalize ) ;
}
//#line 189
float3 TransformWorldToHClipDir ( float3 directionWS , bool doNormalize = false )
{
float3 dirHCS = mul ( ( float3x3 ) GetWorldToHClipMatrix ( ) , directionWS ) . xyz ;
if ( doNormalize )
return normalize ( dirHCS ) ;

return dirHCS ;
}
//#line 199
float3 TransformObjectToWorldNormal ( float3 normalOS , bool doNormalize = true )
{
//#line 205
float3 normalWS = mul ( normalOS , ( float3x3 ) GetWorldToObjectMatrix ( ) ) ;
if ( doNormalize )
return SafeNormalize ( normalWS ) ;

return normalWS ;

}
//#line 214
float3 TransformWorldToObjectNormal ( float3 normalWS , bool doNormalize = true )
{
//#line 220
float3 normalOS = mul ( normalWS , ( float3x3 ) GetObjectToWorldMatrix ( ) ) ;
if ( doNormalize )
return SafeNormalize ( normalOS ) ;

return normalOS ;

}

float3x3 CreateTangentToWorld ( float3 normal , float3 tangent , float flipSign )
{

float sgn = flipSign * GetOddNegativeScale ( ) ;
float3 bitangent = cross ( normal , tangent ) * sgn ;

return float3x3 ( tangent , bitangent , normal ) ;
}
//#line 239
float3 TransformTangentToWorld ( float3 normalTS , float3x3 tangentToWorld , bool doNormalize = false )
{

float3 result = mul ( normalTS , tangentToWorld ) ;
if ( doNormalize )
return SafeNormalize ( result ) ;
return result ;
}
//#line 253
float3 TransformWorldToTangent ( float3 normalWS , float3x3 tangentToWorld , bool doNormalize = true )
{

float3 row0 = tangentToWorld [ 0 ] ;
float3 row1 = tangentToWorld [ 1 ] ;
float3 row2 = tangentToWorld [ 2 ] ;
//#line 261
float3 col0 = cross ( row1 , row2 ) ;
float3 col1 = cross ( row2 , row0 ) ;
float3 col2 = cross ( row0 , row1 ) ;

float determinant = dot ( row0 , col0 ) ;
//#line 270
float3x3 matTBN_I_T = float3x3 ( col0 , col1 , col2 ) ;
float3 result = mul ( matTBN_I_T , normalWS ) ;
if ( doNormalize )
{
float sgn = determinant < 0.0 ? ( - 1.0 ) : 1.0 ;
return SafeNormalize ( sgn * result ) ;
}
else
return result / determinant ;
}
//#line 283
float3 TransformWorldToTangentDir ( float3 dirWS , float3x3 tangentToWorld , bool doNormalize = false )
{

float3 result = mul ( tangentToWorld , dirWS ) ;
if ( doNormalize )
return SafeNormalize ( result ) ;
return result ;
}
//#line 295
float3 TransformTangentToWorldDir ( float3 dirWS , float3x3 tangentToWorld , bool doNormalize = false )
{

float3 row0 = tangentToWorld [ 0 ] ;
float3 row1 = tangentToWorld [ 1 ] ;
float3 row2 = tangentToWorld [ 2 ] ;
//#line 303
float3 col0 = cross ( row1 , row2 ) ;
float3 col1 = cross ( row2 , row0 ) ;
float3 col2 = cross ( row0 , row1 ) ;

float determinant = dot ( row0 , col0 ) ;
//#line 312
float3x3 matTBN_I_T = float3x3 ( col0 , col1 , col2 ) ;
float3 result = mul ( dirWS , matTBN_I_T ) ;
if ( doNormalize )
{
float sgn = determinant < 0.0 ? ( - 1.0 ) : 1.0 ;
return SafeNormalize ( sgn * result ) ;
}
else
return result / determinant ;
}
//#line 324
float3 TransformTangentToObject ( float3 dirTS , float3x3 tangentToWorld )
{

float3 normalWS = TransformTangentToWorld ( dirTS , tangentToWorld ) ;
return TransformWorldToObjectNormal ( normalWS ) ;
}
//#line 332
float3 TransformObjectToTangent ( float3 dirOS , float3x3 tangentToWorld )
{
//#line 337
float3 normalWS = TransformObjectToWorldNormal ( dirOS , false ) ;
//#line 340
return TransformWorldToTangent ( normalWS , tangentToWorld ) ;
}
//#line 206 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct VertexPositionInputs
{
float3 positionWS ;
float3 positionVS ;
float4 positionCS ;
float4 positionNDC ;
} ;

struct VertexNormalInputs
{
float3 tangentWS ;
float3 bitangentWS ;
float3 normalWS ;
} ;
//#line 9 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.deprecated.hlsl"
float4 ComputeScreenPos ( float4 positionCS )
{
float4 o = positionCS * 0.5f ;
o . xy = float2 ( o . x , o . y * _ProjectionParams . x ) + o . w ;
o . zw = positionCS . zw ;
return o ;
}
//#line 251 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/Debug/DebuggingCommon.hlsl"
bool IsAlphaDiscardEnabled ( )
{
//#line 256
return true ;

}

bool IsFogEnabled ( )
{
//#line 272
return true ;

}

bool IsLightingFeatureEnabled ( uint bitMask )
{
//#line 281
return true ;

}

bool IsOnlyAOLightingFeatureEnabled ( )
{
//#line 290
return false ;

}
//#line 7 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
VertexPositionInputs GetVertexPositionInputs ( float3 positionOS )
{
VertexPositionInputs input ;
input . positionWS = TransformObjectToWorld ( positionOS ) ;
input . positionVS = TransformWorldToView ( input . positionWS ) ;
input . positionCS = TransformWorldToHClip ( input . positionWS ) ;

float4 ndc = input . positionCS * 0.5f ;
input . positionNDC . xy = float2 ( ndc . x , ndc . y * _ProjectionParams . x ) + ndc . w ;
input . positionNDC . zw = input . positionCS . zw ;

return input ;
}

VertexNormalInputs GetVertexNormalInputs ( float3 normalOS )
{
VertexNormalInputs tbn ;
tbn . tangentWS = float3 ( 1.0 , 0.0 , 0.0 ) ;
tbn . bitangentWS = float3 ( 0.0 , 1.0 , 0.0 ) ;
tbn . normalWS = TransformObjectToWorldNormal ( normalOS ) ;
return tbn ;
}

VertexNormalInputs GetVertexNormalInputs ( float3 normalOS , float4 tangentOS )
{
VertexNormalInputs tbn ;
//#line 35
float sign = float ( tangentOS . w ) * GetOddNegativeScale ( ) ;
tbn . normalWS = TransformObjectToWorldNormal ( normalOS ) ;
tbn . tangentWS = float3 ( TransformObjectToWorldDir ( tangentOS . xyz ) ) ;
tbn . bitangentWS = float3 ( cross ( tbn . normalWS , float3 ( tbn . tangentWS ) ) ) * sign ;
return tbn ;
}

float4 GetScaledScreenParams ( )
{
return _ScaledScreenParams ;
}
//#line 48
bool IsPerspectiveProjection ( )
{
return ( unity_OrthoParams . w == 0 ) ;
}

float3 GetCameraPositionWS ( )
{
//#line 57
return _WorldSpaceCameraPos ;
//#line 66
}
//#line 69
float3 GetCurrentViewPosition ( )
{
//#line 73
return GetCameraPositionWS ( ) ;
//#line 85
}
//#line 88
float3 GetViewForwardDir ( )
{
float4x4 viewMat = GetWorldToViewMatrix ( ) ;
return - viewMat [ 2 ] . xyz ;
}
//#line 95
float3 GetWorldSpaceViewDir ( float3 positionWS )
{
if ( IsPerspectiveProjection ( ) )
{

return GetCurrentViewPosition ( ) - positionWS ;
}
else
{

return - GetViewForwardDir ( ) ;
}
}
//#line 110
half3 GetObjectSpaceNormalizeViewDir ( float3 positionOS )
{
if ( IsPerspectiveProjection ( ) )
{

float3 V = TransformWorldToObject ( GetCurrentViewPosition ( ) ) - positionOS ;
return half3 ( normalize ( V ) ) ;
}
else
{

return half3 ( TransformWorldToObjectNormal ( - GetViewForwardDir ( ) ) ) ;
}
}

half3 GetWorldSpaceNormalizeViewDir ( float3 positionWS )
{
if ( IsPerspectiveProjection ( ) )
{

float3 V = GetCurrentViewPosition ( ) - positionWS ;
return half3 ( normalize ( V ) ) ;
}
else
{

return half3 ( - GetViewForwardDir ( ) ) ;
}
}
//#line 143
void GetLeftHandedViewSpaceMatrices ( out float4x4 viewMatrix , out float4x4 projMatrix )
{
viewMatrix = unity_MatrixV ;
viewMatrix . _31_32_33_34 = - viewMatrix . _31_32_33_34 ;

projMatrix = OptimizeProjectionMatrix ( glstate_matrix_projection ) ;
projMatrix . _13_23_33_43 = - projMatrix . _13_23_33_43 ;
}
//#line 155
static const half kSurfaceTypeOpaque = 0.0 ;
static const half kSurfaceTypeTransparent = 1.0 ;
//#line 159
bool IsSurfaceTypeOpaque ( half surfaceType )
{
return ( surfaceType == kSurfaceTypeOpaque ) ;
}
//#line 165
bool IsSurfaceTypeTransparent ( half surfaceType )
{
return ( surfaceType == kSurfaceTypeTransparent ) ;
}
//#line 218
float AlphaDiscard ( float alpha , float cutoff , float offset = float ( 0.0 ) )
{
//#line 225
return alpha ;
}

half OutputAlpha ( half alpha , bool isTransparent )
{
if ( isTransparent )
{
return alpha ;
}
else
{
//#line 240
return 1.0 ;

}
}

half3 AlphaModulate ( half3 albedo , half alpha )
{
//#line 254
return albedo ;

}

half3 AlphaPremultiply ( half3 albedo , half alpha )
{
//#line 267
return albedo ;
}
//#line 273
half3 NormalizeNormalPerVertex ( half3 normalWS )
{
return normalize ( normalWS ) ;
}

float3 NormalizeNormalPerVertex ( float3 normalWS )
{
return normalize ( normalWS ) ;
}

half3 NormalizeNormalPerPixel ( half3 normalWS )
{
//#line 289
return normalize ( normalWS ) ;

}

float3 NormalizeNormalPerPixel ( float3 normalWS )
{
//#line 298
return normalize ( normalWS ) ;

}
//#line 304
float ComputeFogFactorZ0ToFar ( float z )
{
//#line 315
return float ( 0.0 ) ;

}

float ComputeFogFactor ( float zPositionCS )
{
float clipZ_0Far = max ( ( ( 1.0 - ( zPositionCS ) / _ProjectionParams . y ) * _ProjectionParams . z ) , 0 ) ;
return ComputeFogFactorZ0ToFar ( clipZ_0Far ) ;
}

half ComputeFogIntensity ( half fogFactor )
{
half fogIntensity = half ( 0.0 ) ;
//#line 341
return fogIntensity ;
}
//#line 346
float InitializeInputDataFog ( float4 positionWS , float vertFogFactor )
{
float fogFactor = 0.0 ;
//#line 360
return fogFactor ;
}

float ComputeFogIntensity ( float fogFactor )
{
float fogIntensity = 0.0 ;
//#line 379
return fogIntensity ;
}

half3 MixFogColor ( half3 fragColor , half3 fogColor , half fogFactor )
{
//#line 388
return fragColor ;
}

float3 MixFogColor ( float3 fragColor , float3 fogColor , float fogFactor )
{
//#line 400
return fragColor ;
}

half3 MixFog ( half3 fragColor , half fogFactor )
{
return MixFogColor ( fragColor , half3 ( unity_FogColor . rgb ) , fogFactor ) ;
}

float3 MixFog ( float3 fragColor , float fogFactor )
{
return MixFogColor ( fragColor , unity_FogColor . rgb , fogFactor ) ;
}
//#line 414
half LinearDepthToEyeDepth ( half rawDepth )
{

return half ( _ProjectionParams . z - ( _ProjectionParams . z - _ProjectionParams . y ) * rawDepth ) ;
//#line 421
}

float LinearDepthToEyeDepth ( float rawDepth )
{

return _ProjectionParams . z - ( _ProjectionParams . z - _ProjectionParams . y ) * rawDepth ;
//#line 430
}

void TransformScreenUV ( inout float2 uv , float screenHeight )
{

uv . y = screenHeight - ( uv . y * _ScaleBiasRt . x + _ScaleBiasRt . y * screenHeight ) ;

}

void TransformScreenUV ( inout float2 uv )
{

TransformScreenUV ( uv , GetScaledScreenParams ( ) . y ) ;

}

void TransformNormalizedScreenUV ( inout float2 uv )
{

TransformScreenUV ( uv , 1.0 ) ;

}

float2 GetNormalizedScreenSpaceUV ( float2 positionCS )
{
float2 normalizedScreenSpaceUV = positionCS . xy * rcp ( GetScaledScreenParams ( ) . xy ) ;
TransformNormalizedScreenUV ( normalizedScreenSpaceUV ) ;
return normalizedScreenSpaceUV ;
}

float2 GetNormalizedScreenSpaceUV ( float4 positionCS )
{
return GetNormalizedScreenSpaceUV ( positionCS . xy ) ;
}
//#line 471
uint Select4 ( uint4 v , uint i )
{
//#line 477
uint mask0 = uint ( int ( i << 31 ) >> 31 ) ;
uint mask1 = uint ( int ( i << 30 ) >> 31 ) ;
return
( ( ( v . w & mask0 ) | ( v . z & ~ mask0 ) ) & mask1 ) |
( ( ( v . y & mask0 ) | ( v . x & ~ mask0 ) ) & ~ mask1 ) ;
}
//#line 497
uint GetMeshRenderingLayer ( )
{
return asuint ( unity_RenderingLayer . x ) ;
}

float EncodeMeshRenderingLayer ( uint renderingLayer )
{

renderingLayer &= _RenderingLayerMaxInt ;
//#line 511
float rcpMaxInt = _RenderingLayerRcpMaxInt ;
return saturate ( renderingLayer * rcpMaxInt ) ;
}

uint DecodeMeshRenderingLayer ( float renderingLayer )
{
//#line 521
uint maxInt = _RenderingLayerMaxInt ;
return ( uint ) ( renderingLayer * maxInt + 0.5 ) ;
}
//#line 526
float GetCurrentExposureMultiplier ( )
{
return 1 ;
}
//#line 29 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
float PerceptualRoughnessToRoughness ( float perceptualRoughness )
{
return perceptualRoughness * perceptualRoughness ;
}

float RoughnessToPerceptualRoughness ( float roughness )
{
return sqrt ( roughness ) ;
}
//#line 40
float RoughnessToPerceptualSmoothness ( float roughness )
{
return 1.0 - sqrt ( roughness ) ;
}

float PerceptualSmoothnessToRoughness ( float perceptualSmoothness )
{
return ( 1.0 - perceptualSmoothness ) * ( 1.0 - perceptualSmoothness ) ;
}

float PerceptualSmoothnessToPerceptualRoughness ( float perceptualSmoothness )
{
return ( 1.0 - perceptualSmoothness ) ;
}
//#line 82
float BeckmannRoughnessToGGXRoughness ( float roughnessBeckmann )
{
return 0.5 * roughnessBeckmann ;
}

float PerceptualRoughnessBeckmannToGGX ( float perceptualRoughnessBeckmann )
{

return sqrt ( 0.5 ) * perceptualRoughnessBeckmann ;
}

float GGXRoughnessToBeckmannRoughness ( float roughnessGGX )
{
return 2.0 * roughnessGGX ;
}

float PerceptualRoughnessToPerceptualSmoothness ( float perceptualRoughness )
{
return ( 1.0 - perceptualRoughness ) ;
}
//#line 107
float ClampRoughnessForAnalyticalLights ( float roughness )
{
return max ( roughness , 1.0 / 1024.0 ) ;
}
//#line 114
float ClampRoughnessForRaytracing ( float roughness )
{
return max ( roughness , 0.001225 ) ;
}
float ClampPerceptualRoughnessForRaytracing ( float perceptualRoughness )
{
return max ( perceptualRoughness , 0.035 ) ;
}

void ConvertValueAnisotropyToValueTB ( float value , float anisotropy , out float valueT , out float valueB )
{
//#line 127
valueT = value * ( 1 + anisotropy ) ;
valueB = value * ( 1 - anisotropy ) ;
}

void ConvertAnisotropyToRoughness ( float perceptualRoughness , float anisotropy , out float roughnessT , out float roughnessB )
{
float roughness = PerceptualRoughnessToRoughness ( perceptualRoughness ) ;
ConvertValueAnisotropyToValueTB ( roughness , anisotropy , roughnessT , roughnessB ) ;
}

void ConvertRoughnessTAndAnisotropyToRoughness ( float roughnessT , float anisotropy , out float roughness )
{
roughness = roughnessT / ( 1 + anisotropy ) ;
}

float ConvertRoughnessTAndBToRoughness ( float roughnessT , float roughnessB )
{
return 0.5 * ( roughnessT + roughnessB ) ;
}

void ConvertRoughnessToAnisotropy ( float roughnessT , float roughnessB , out float anisotropy )
{
anisotropy = ( ( roughnessT - roughnessB ) / max ( roughnessT + roughnessB , 0.0001 ) ) ;
}
//#line 155
void ConvertAnisotropyToClampRoughness ( float perceptualRoughness , float anisotropy , out float roughnessT , out float roughnessB )
{
ConvertAnisotropyToRoughness ( perceptualRoughness , anisotropy , roughnessT , roughnessB ) ;

roughnessT = ClampRoughnessForAnalyticalLights ( roughnessT ) ;
roughnessB = ClampRoughnessForAnalyticalLights ( roughnessB ) ;
}
//#line 164
float RoughnessToVariance ( float roughness )
{
return 2.0 / Sq ( roughness ) - 2.0 ;
}

float VarianceToRoughness ( float variance )
{
return sqrt ( 2.0 / ( variance + 2.0 ) ) ;
}
//#line 177
float DecodeVariance ( float gradientW )
{
return gradientW * 0.03125 ;
}
//#line 183
float NormalFiltering ( float perceptualSmoothness , float variance , float threshold )
{
float roughness = PerceptualSmoothnessToRoughness ( perceptualSmoothness ) ;

float squaredRoughness = saturate ( roughness * roughness + min ( 2.0 * variance , threshold * threshold ) ) ;

return RoughnessToPerceptualSmoothness ( sqrt ( squaredRoughness ) ) ;
}

float ProjectedSpaceNormalFiltering ( float perceptualSmoothness , float variance , float threshold )
{
float roughness = PerceptualSmoothnessToRoughness ( perceptualSmoothness ) ;

float squaredRoughness = roughness * roughness ;
float projRoughness2 = squaredRoughness / ( 1.0 - squaredRoughness ) ;
float filteredProjRoughness2 = saturate ( projRoughness2 + min ( 2.0 * variance , threshold * threshold ) ) ;
squaredRoughness = filteredProjRoughness2 / ( filteredProjRoughness2 + 1.0f ) ;

return RoughnessToPerceptualSmoothness ( sqrt ( squaredRoughness ) ) ;
}
//#line 209
float GeometricNormalVariance ( float3 geometricNormalWS , float screenSpaceVariance )
{
float3 deltaU = ddx ( geometricNormalWS ) ;
float3 deltaV = ddy ( geometricNormalWS ) ;

return screenSpaceVariance * ( dot ( deltaU , deltaU ) + dot ( deltaV , deltaV ) ) ;
}
//#line 218
float GeometricNormalFiltering ( float perceptualSmoothness , float3 geometricNormalWS , float screenSpaceVariance , float threshold )
{
float variance = GeometricNormalVariance ( geometricNormalWS , screenSpaceVariance ) ;
return NormalFiltering ( perceptualSmoothness , variance , threshold ) ;
}

float ProjectedSpaceGeometricNormalFiltering ( float perceptualSmoothness , float3 geometricNormalWS , float screenSpaceVariance , float threshold )
{
float variance = GeometricNormalVariance ( geometricNormalWS , screenSpaceVariance ) ;
return ProjectedSpaceNormalFiltering ( perceptualSmoothness , variance , threshold ) ;
}
//#line 241
float TextureNormalVariance ( float avgNormalLength )
{
float variance = 0.0 ;

if ( avgNormalLength < 1.0 )
{
float avgNormLen2 = avgNormalLength * avgNormalLength ;
float kappa = ( 3.0 * avgNormalLength - avgNormalLength * avgNormLen2 ) / ( 1.0 - avgNormLen2 ) ;
//#line 257
variance = 0.25 / kappa ;
}

return variance ;
}

float TextureNormalFiltering ( float perceptualSmoothness , float avgNormalLength , float threshold )
{
float variance = TextureNormalVariance ( avgNormalLength ) ;
return NormalFiltering ( perceptualSmoothness , variance , threshold ) ;
}
//#line 273
float3 ComputeDiffuseColor ( float3 baseColor , float metallic )
{
return baseColor * ( 1.0 - metallic ) ;
}

float3 ComputeFresnel0 ( float3 baseColor , float metallic , float dielectricF0 )
{
return lerp ( dielectricF0 . xxx , baseColor , metallic ) ;
}
//#line 291
float3 BlendNormalWorldspaceRNM ( float3 n1 , float3 n2 , float3 vtxNormal )
{

float4 q = float4 ( cross ( vtxNormal , n2 ) , dot ( vtxNormal , n2 ) + 1.0 ) / sqrt ( 2.0 * ( dot ( vtxNormal , n2 ) + 1 ) ) ;
//#line 297
return n1 * ( q . w * q . w - dot ( q . xyz , q . xyz ) ) + 2 * q . xyz * dot ( q . xyz , n1 ) + 2 * q . w * cross ( q . xyz , n1 ) ;
}
//#line 305
float3 BlendNormalRNM ( float3 n1 , float3 n2 )
{
float3 t = n1 . xyz + float3 ( 0.0 , 0.0 , 1.0 ) ;
float3 u = n2 . xyz * float3 ( - 1.0 , - 1.0 , 1.0 ) ;
float3 r = ( t / t . z ) * dot ( t , u ) - u ;
return r ;
}
//#line 314
float3 BlendNormal ( float3 n1 , float3 n2 )
{
return normalize ( float3 ( n1 . xy * n2 . z + n2 . xy * n1 . z , n1 . z * n2 . z ) ) ;
}
//#line 324
float3 ComputeTriplanarWeights ( float3 normal )
{

float3 blendWeights = abs ( normal ) ;

blendWeights = ( blendWeights - 0.2 ) ;
blendWeights = blendWeights * blendWeights * blendWeights ;

blendWeights = max ( blendWeights , float3 ( 0.0 , 0.0 , 0.0 ) ) ;
blendWeights /= dot ( blendWeights , 1.0 ) ;

return blendWeights ;
}
//#line 339
void GetTriplanarCoordinate ( float3 position , out float2 uvXZ , out float2 uvXY , out float2 uvZY )
{
//#line 343
uvXZ = float2 ( position . x , position . z ) ;
uvXY = float2 ( position . x , position . y ) ;
uvZY = float2 ( position . z , position . y ) ;
}
//#line 352
float LerpWhiteTo ( float b , float t )
{
float oneMinusT = 1.0 - t ;
return oneMinusT + b * t ;
}
//#line 359
float3 LerpWhiteTo ( float3 b , float t )
{
float oneMinusT = 1.0 - t ;
return float3 ( oneMinusT , oneMinusT , oneMinusT ) + b * t ;
}
//#line 5 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
struct SurfaceData
{
half3 albedo ;
half3 specular ;
half metallic ;
half smoothness ;
half3 normalTS ;
half3 emission ;
half occlusion ;
half alpha ;
half clearCoatMask ;
half clearCoatSmoothness ;
} ;
//#line 10 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
Texture2D _BaseMap ;
SamplerState sampler_BaseMap ;
float4 _BaseMap_TexelSize ;
float4 _BaseMap_MipInfo ; float4 _BaseMap_StreamInfo ; ;
Texture2D _BumpMap ;
SamplerState sampler_BumpMap ;
Texture2D _EmissionMap ;
SamplerState sampler_EmissionMap ;
//#line 22
half Alpha ( half albedoAlpha , half4 color , half cutoff )
{

half alpha = albedoAlpha * color . a ;
//#line 30
alpha = AlphaDiscard ( alpha , cutoff ) ;

return alpha ;
}

half4 SampleAlbedoAlpha ( float2 uv , Texture2D albedoAlphaMap , SamplerState sampler_albedoAlphaMap )
{
return half4 ( albedoAlphaMap . SampleBias ( sampler_albedoAlphaMap , uv , _GlobalMipBias . x ) ) ;
}

half3 SampleNormal ( float2 uv , Texture2D bumpMap , SamplerState sampler_bumpMap , half scale = half ( 1.0 ) )
{
//#line 50
return half3 ( 0.0h , 0.0h , 1.0h ) ;

}

half3 SampleEmission ( float2 uv , half3 emissionColor , Texture2D emissionMap , SamplerState sampler_emissionMap )
{

return 0 ;
//#line 61
}
//#line 5 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
half3 GetViewDirectionTangentSpace ( half4 tangentWS , half3 normalWS , half3 viewDirWS )
{

half3 unnormalizedNormalWS = normalWS ;
const half renormFactor = 1.0 / length ( unnormalizedNormalWS ) ;
//#line 13
half crossSign = ( tangentWS . w > 0.0 ? 1.0 : - 1.0 ) ;
half3 bitang = crossSign * cross ( normalWS . xyz , tangentWS . xyz ) ;

half3 WorldSpaceNormal = renormFactor * normalWS . xyz ;
//#line 20
half3 WorldSpaceTangent = renormFactor * tangentWS . xyz ;
half3 WorldSpaceBiTangent = renormFactor * bitang ;

half3x3 tangentSpaceTransform = half3x3 ( WorldSpaceTangent , WorldSpaceBiTangent , WorldSpaceNormal ) ;
half3 viewDirTS = mul ( tangentSpaceTransform , viewDirWS ) ;

return viewDirTS ;
}
//#line 30
half2 ParallaxOffset1Step ( half height , half amplitude , half3 viewDirTS )
{
height = height * amplitude - amplitude / 2.0 ;
half3 v = normalize ( viewDirTS ) ;
v . z += 0.42 ;
return height * ( v . xy / v . z ) ;
}
//#line 39
float2 ParallaxMapping ( Texture2D heightMap , SamplerState sampler_heightMap , half3 viewDirTS , half scale , float2 uv )
{
half h = heightMap . SampleBias ( sampler_heightMap , uv , _GlobalMipBias . x ) . g ;
float2 offset = ParallaxOffset1Step ( h , scale , viewDirTS ) ;
return offset ;
}

float2 ParallaxMappingChannel ( Texture2D heightMap , SamplerState sampler_heightMap , half3 viewDirTS , half scale , float2 uv , int channel )
{
half h = heightMap . SampleBias ( sampler_heightMap , uv , _GlobalMipBias . x ) [ channel ] ;
float2 offset = ParallaxOffset1Step ( h , scale , viewDirTS ) ;
return offset ;
}
//#line 4 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
struct DecalSurfaceData
{
half4 baseColor ;
half4 normalWS ;
half3 emissive ;
half metallic ;
half occlusion ;
half smoothness ;
half MAOSAlpha ;
} ;
//#line 72 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
void EncodeIntoDBuffer ( DecalSurfaceData surfaceData
, out half4 outDBuffer0
//#line 80
)
{
outDBuffer0 = surfaceData . baseColor ;
//#line 89
}

void DecodeFromDBuffer (
half4 inDBuffer0
//#line 99
, out DecalSurfaceData surfaceData
)
{
surfaceData = ( DecalSurfaceData ) 0 ; ;
surfaceData . baseColor = inDBuffer0 ;
//#line 116
}

Texture2D _DBufferTexture0 ; ;

void ApplyDecal ( float4 positionCS ,
inout half3 baseColor ,
inout half3 specularColor ,
inout half3 normalWS ,
inout half metallic ,
inout half occlusion ,
inout half smoothness )
{
half4 DBuffer0 = _DBufferTexture0 . Load ( int3 ( int2 ( positionCS . xy ) , 0 ) ) ; ;

DecalSurfaceData decalSurfaceData ;
DecodeFromDBuffer ( DBuffer0 , decalSurfaceData ) ;
//#line 136
baseColor . xyz = baseColor . xyz * decalSurfaceData . baseColor . w + decalSurfaceData . baseColor . xyz ;
//#line 161
}

void ApplyDecalToBaseColor ( float4 positionCS , inout half3 baseColor )
{
half4 DBuffer0 = _DBufferTexture0 . Load ( int3 ( int2 ( positionCS . xy ) , 0 ) ) ; ;

DecalSurfaceData decalSurfaceData ;
DecodeFromDBuffer ( DBuffer0 , decalSurfaceData ) ;
//#line 173
baseColor . xyz = baseColor . xyz * decalSurfaceData . baseColor . w + decalSurfaceData . baseColor . xyz ;
}

void ApplyDecalToBaseColorAndNormal ( float4 positionCS , inout half3 baseColor , inout half3 normalWS )
{
half3 specular = 0 ;
half metallic = 0 ;
half occlusion = 0 ;
half smoothness = 0 ;
ApplyDecal ( positionCS ,
baseColor ,
specular ,
normalWS ,
metallic ,
occlusion ,
smoothness ) ;
}

void ApplyDecalToSurfaceData ( float4 positionCS , inout SurfaceData surfaceData , inout InputData inputData )
{
//#line 203
half3 specular = 0 ;
ApplyDecal ( positionCS ,
surfaceData . albedo ,
specular ,
inputData . normalWS ,
surfaceData . metallic ,
surfaceData . occlusion ,
surfaceData . smoothness ) ;

}
//#line 16 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
cbuffer UnityPerMaterial {
float4 _BaseMap_ST ;
float4 _DetailAlbedoMap_ST ;
half4 _BaseColor ;
half4 _SpecColor ;
half4 _EmissionColor ;
half _Cutoff ;
half _Smoothness ;
half _Metallic ;
half _BumpScale ;
half _Parallax ;
half _OcclusionStrength ;
half _ClearCoatMask ;
half _ClearCoatSmoothness ;
half _DetailAlbedoMapScale ;
half _DetailNormalMapScale ;
half _Surface ;
float4 unity_MipmapStreaming_DebugTex_ST ; float4 unity_MipmapStreaming_DebugTex_TexelSize ; float4 unity_MipmapStreaming_DebugTex_MipInfo ; float4 unity_MipmapStreaming_DebugTex_StreamInfo ; ;
} ;
//#line 121
Texture2D _ParallaxMap ; SamplerState sampler_ParallaxMap ;
Texture2D _OcclusionMap ; SamplerState sampler_OcclusionMap ;
Texture2D _DetailMask ; SamplerState sampler_DetailMask ;
Texture2D _DetailAlbedoMap ; SamplerState sampler_DetailAlbedoMap ;
Texture2D _DetailNormalMap ; SamplerState sampler_DetailNormalMap ;
Texture2D _MetallicGlossMap ; SamplerState sampler_MetallicGlossMap ;
Texture2D _SpecGlossMap ; SamplerState sampler_SpecGlossMap ;
Texture2D _ClearCoatMap ; SamplerState sampler_ClearCoatMap ;
//#line 136
half4 SampleMetallicSpecGloss ( float2 uv , half albedoAlpha )
{
half4 specGloss ;
//#line 151
specGloss . rgb = _Metallic . rrr ;
//#line 157
specGloss . a = _Smoothness ;
//#line 161
return specGloss ;
}

half SampleOcclusion ( float2 uv )
{
//#line 170
return half ( 1.0 ) ;

}
//#line 178
half2 SampleClearCoat ( float2 uv )
{
//#line 189
return half2 ( 0.0 , 1.0 ) ;

}

void ApplyPerPixelDisplacement ( half3 viewDirTS , inout float2 uv )
{
//#line 198
}
//#line 203
half3 ScaleDetailAlbedo ( half3 detailAlbedo , half scale )
{
//#line 211
return half ( 2.0 ) * detailAlbedo * scale - scale + half ( 1.0 ) ;
}

half3 ApplyDetailAlbedo ( float2 detailUv , half3 albedo , half detailMask )
{
//#line 228
return albedo ;

}

half3 ApplyDetailNormal ( float2 detailUv , half3 normalTS , half detailMask )
{
//#line 247
return normalTS ;

}

inline void InitializeStandardLitSurfaceData ( float2 uv , out SurfaceData outSurfaceData )
{
half4 albedoAlpha = SampleAlbedoAlpha ( uv , _BaseMap , sampler_BaseMap ) ;
outSurfaceData . alpha = Alpha ( albedoAlpha . a , _BaseColor , _Cutoff ) ;

half4 specGloss = SampleMetallicSpecGloss ( uv , albedoAlpha . a ) ;
outSurfaceData . albedo = albedoAlpha . rgb * _BaseColor . rgb ;
outSurfaceData . albedo = AlphaModulate ( outSurfaceData . albedo , outSurfaceData . alpha ) ;
//#line 264
outSurfaceData . metallic = specGloss . r ;
outSurfaceData . specular = half3 ( 0.0 , 0.0 , 0.0 ) ;
//#line 268
outSurfaceData . smoothness = specGloss . a ;
outSurfaceData . normalTS = SampleNormal ( uv , _BumpMap , sampler_BumpMap , _BumpScale ) ;
outSurfaceData . occlusion = SampleOcclusion ( uv ) ;
outSurfaceData . emission = SampleEmission ( uv , _EmissionColor . rgb , _EmissionMap , sampler_EmissionMap ) ;
//#line 278
outSurfaceData . clearCoatMask = half ( 0.0 ) ;
outSurfaceData . clearCoatSmoothness = half ( 0.0 ) ;
//#line 288
}
//#line 70 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/ACES.hlsl"
static const half3x3 sRGB_2_AP0 = {
0.4397010 , 0.3829780 , 0.1773350 ,
0.0897923 , 0.8134230 , 0.0967616 ,
0.0175440 , 0.1115440 , 0.8707040
} ;

static const half3x3 sRGB_2_AP1 = {
0.61319 , 0.33951 , 0.04737 ,
0.07021 , 0.91634 , 0.01345 ,
0.02062 , 0.10957 , 0.86961
} ;

static const half3x3 AP0_2_sRGB = {
2.52169 , - 1.13413 , - 0.38756 ,
- 0.27648 , 1.37272 , - 0.09624 ,
- 0.01538 , - 0.15298 , 1.16835 ,
} ;

static const half3x3 AP1_2_sRGB = {
1.70505 , - 0.62179 , - 0.08326 ,
- 0.13026 , 1.14080 , - 0.01055 ,
- 0.02400 , - 0.12897 , 1.15297 ,
} ;

static const half3x3 AP0_2_AP1_MAT = {
1.4514393161 , - 0.2365107469 , - 0.2149285693 ,
- 0.0765537734 , 1.1762296998 , - 0.0996759264 ,
0.0083161484 , - 0.0060324498 , 0.9977163014
} ;

static const half3x3 AP1_2_AP0_MAT = {
0.6954522414 , 0.1406786965 , 0.1638690622 ,
0.0447945634 , 0.8596711185 , 0.0955343182 ,
- 0.0055258826 , 0.0040252103 , 1.0015006723
} ;

static const half3x3 AP1_2_XYZ_MAT = {
0.6624541811 , 0.1340042065 , 0.1561876870 ,
0.2722287168 , 0.6740817658 , 0.0536895174 ,
- 0.0055746495 , 0.0040607335 , 1.0103391003
} ;

static const half3x3 XYZ_2_AP1_MAT = {
1.6410233797 , - 0.3248032942 , - 0.2364246952 ,
- 0.6636628587 , 1.6153315917 , 0.0167563477 ,
0.0117218943 , - 0.0082844420 , 0.9883948585
} ;

static const half3x3 XYZ_2_REC709_MAT = {
3.2409699419 , - 1.5373831776 , - 0.4986107603 ,
- 0.9692436363 , 1.8759675015 , 0.0415550574 ,
0.0556300797 , - 0.2039769589 , 1.0569715142
} ;

static const half3x3 XYZ_2_REC2020_MAT = {
1.7166511880 , - 0.3556707838 , - 0.2533662814 ,
- 0.6666843518 , 1.6164812366 , 0.0157685458 ,
0.0176398574 , - 0.0427706133 , 0.9421031212
} ;

static const half3x3 XYZ_2_DCIP3_MAT = {
2.7253940305 , - 1.0180030062 , - 0.4401631952 ,
- 0.7951680258 , 1.6897320548 , 0.0226471906 ,
0.0412418914 , - 0.0876390192 , 1.1009293786
} ;

static const half3x3 XYZ_2_P3D65_MAT = {
2.4934969119 , - 0.9313836179 , - 0.4027107845 ,
- 0.8294889696 , 1.7626640603 , 0.0236246858 ,
0.0358458302 , - 0.0761723893 , 0.9568845240
} ;

static const half3 AP1_RGB2Y = half3 ( 0.272229 , 0.674082 , 0.0536895 ) ;

static const half3x3 RRT_SAT_MAT = {
0.9708890 , 0.0269633 , 0.00214758 ,
0.0108892 , 0.9869630 , 0.00214758 ,
0.0108892 , 0.0269633 , 0.96214800
} ;

static const half3x3 ODT_SAT_MAT = {
0.949056 , 0.0471857 , 0.00375827 ,
0.019056 , 0.9771860 , 0.00375827 ,
0.019056 , 0.0471857 , 0.93375800
} ;

static const half3x3 D60_2_D65_CAT = {
0.98722400 , - 0.00611327 , 0.0159533 ,
- 0.00759836 , 1.00186000 , 0.0053302 ,
0.00307257 , - 0.00509595 , 1.0816800
} ;
//#line 168
half3 unity_to_ACES ( half3 x )
{
x = mul ( sRGB_2_AP0 , x ) ;
return x ;
}
//#line 180
half3 ACES_to_unity ( half3 x )
{
x = mul ( AP0_2_sRGB , x ) ;
return x ;
}
//#line 192
half3 unity_to_ACEScg ( half3 x )
{
x = mul ( sRGB_2_AP1 , x ) ;
return x ;
}
//#line 204
half3 ACEScg_to_unity ( half3 x )
{
x = mul ( AP1_2_sRGB , x ) ;
return x ;
}

half3 ACEScg_to_Rec2020 ( half3 x )
{
half3 xyz = mul ( AP1_2_XYZ_MAT , x ) ;
return mul ( XYZ_2_REC2020_MAT , xyz ) ;
}
//#line 224
half ACES_to_ACEScc ( half x )
{
if ( x <= 0.0 )
return - 0.35828683 ;
else if ( x < pow ( 2.0 , - 15.0 ) )
return ( log2 ( pow ( 2.0 , - 16.0 ) + x * 0.5 ) + 9.72 ) / 17.52 ;
else
return ( log2 ( x ) + 9.72 ) / 17.52 ;
}

half ACES_to_ACEScc_fast ( half x )
{

return ( x < 0.00003051757 ) ? ( log2 ( 0.00001525878 + x * 0.5 ) + 9.72 ) / 17.52 : ( log2 ( x ) + 9.72 ) / 17.52 ;
}

half3 ACES_to_ACEScc ( half3 x )
{
x = clamp ( x , 0.0 , 65504.0 ) ;
//#line 245
return half3 (
ACES_to_ACEScc_fast ( x . r ) ,
ACES_to_ACEScc_fast ( x . g ) ,
ACES_to_ACEScc_fast ( x . b )
) ;
//#line 258
}
//#line 268
half ACEScc_to_ACES ( half x )
{

if ( x < - 0.3013698630 )
return ( pow ( 2.0 , x * 17.52 - 9.72 ) - pow ( 2.0 , - 16.0 ) ) * 2.0 ;
else if ( x < ( log2 ( 65504.0 ) + 9.72 ) / 17.52 )
return pow ( 2.0 , x * 17.52 - 9.72 ) ;
else
return 65504.0 ;
}

half3 ACEScc_to_ACES ( half3 x )
{
return half3 (
ACEScc_to_ACES ( x . r ) ,
ACEScc_to_ACES ( x . g ) ,
ACEScc_to_ACES ( x . b )
) ;
}
//#line 296
float3 ACES_to_ACEScg ( float3 x )
{
return mul ( AP0_2_AP1_MAT , x ) ;
}
//#line 309
float3 ACEScg_to_ACES ( float3 x )
{
return mul ( AP1_2_AP0_MAT , x ) ;
}
//#line 320
half rgb_2_saturation ( half3 rgb )
{
const half TINY = 1e-4 ;
half mi = Min3 ( rgb . r , rgb . g , rgb . b ) ;
half ma = Max3 ( rgb . r , rgb . g , rgb . b ) ;
return ( max ( ma , TINY ) - max ( mi , TINY ) ) / max ( ma , 1e-2 ) ;
}

half rgb_2_yc ( half3 rgb )
{
const half ycRadiusWeight = 1.75 ;
//#line 345
half r = rgb . x ;
half g = rgb . y ;
half b = rgb . z ;
half k = b * ( b - g ) + g * ( g - r ) + r * ( r - b ) ;
k = max ( k , 0.0 ) ;
//#line 353
half chroma = sqrt ( k ) ;

return ( b + g + r + ycRadiusWeight * chroma ) / 3.0 ;
}

half rgb_2_hue ( half3 rgb )
{
//#line 362
half hue ;
if ( rgb . x == rgb . y && rgb . y == rgb . z )
hue = 0.0 ;
else
hue = ( 180.0 / 3.14159265358979323846 ) * atan2 ( sqrt ( 3.0 ) * ( rgb . y - rgb . z ) , 2.0 * rgb . x - rgb . y - rgb . z ) ;

if ( hue < 0.0 ) hue = hue + 360.0 ;

return hue ;
}

half center_hue ( half hue , half centerH )
{
half hueCentered = hue - centerH ;
if ( hueCentered < - 180.0 ) hueCentered = hueCentered + 360.0 ;
else if ( hueCentered > 180.0 ) hueCentered = hueCentered - 360.0 ;
return hueCentered ;
}

half sigmoid_shaper ( half x )
{
//#line 385
half t = max ( 1.0 - abs ( x / 2.0 ) , 0.0 ) ;
half y = 1.0 + half ( FastSign ( x ) ) * ( 1.0 - t * t ) ;

return y / 2.0 ;
}

half glow_fwd ( half ycIn , half glowGainIn , half glowMid )
{
half glowGainOut ;

if ( ycIn <= 2.0 / 3.0 * glowMid )
glowGainOut = glowGainIn ;
else if ( ycIn >= 2.0 * glowMid )
glowGainOut = 0.0 ;
else
glowGainOut = glowGainIn * ( glowMid / ycIn - 1.0 / 2.0 ) ;

return glowGainOut ;
}
//#line 467
static const half3x3 M = {
0.5 , - 1.0 , 0.5 ,
- 1.0 , 1.0 , 0.0 ,
0.5 , 0.5 , 0.0
} ;

half segmented_spline_c5_fwd ( half x )
{
const half coefsLow [ 6 ] = { - 4.0000000000 , - 4.0000000000 , - 3.1573765773 , - 0.4852499958 , 1.8477324706 , 1.8477324706 } ;
const half coefsHigh [ 6 ] = { - 0.7185482425 , 2.0810307172 , 3.6681241237 , 4.0000000000 , 4.0000000000 , 4.0000000000 } ;
const half2 minPoint = half2 ( 0.18 * exp2 ( - 15.0 ) , 0.0001 ) ;
const half2 midPoint = half2 ( 0.18 , 0.48 ) ;
const half2 maxPoint = half2 ( 0.18 * exp2 ( 18.0 ) , 10000.0 ) ;
const half slopeLow = 0.0 ;
const half slopeHigh = 0.0 ;

const int N_KNOTS_LOW = 4 ;
const int N_KNOTS_HIGH = 4 ;
//#line 488
half xCheck = x ;
if ( xCheck <= 0.0 ) xCheck = 6.103515625e-5 ;

half logx = log10 ( xCheck ) ;
half logy ;

if ( logx <= log10 ( minPoint . x ) )
{
logy = logx * slopeLow + ( log10 ( minPoint . y ) - slopeLow * log10 ( minPoint . x ) ) ;
}
else if ( ( logx > log10 ( minPoint . x ) ) && ( logx < log10 ( midPoint . x ) ) )
{
half knot_coord = half ( N_KNOTS_LOW - 1 ) * ( logx - log10 ( minPoint . x ) ) / ( log10 ( midPoint . x ) - log10 ( minPoint . x ) ) ;
int j = knot_coord ;
half t = knot_coord - half ( j ) ;

half3 cf = half3 ( coefsLow [ j ] , coefsLow [ j + 1 ] , coefsLow [ j + 2 ] ) ;
half3 monomials = half3 ( t * t , t , 1.0 ) ;
logy = dot ( monomials , mul ( M , cf ) ) ;
}
else if ( ( logx >= log10 ( midPoint . x ) ) && ( logx < log10 ( maxPoint . x ) ) )
{
half knot_coord = half ( N_KNOTS_HIGH - 1 ) * ( logx - log10 ( midPoint . x ) ) / ( log10 ( maxPoint . x ) - log10 ( midPoint . x ) ) ;
int j = knot_coord ;
half t = knot_coord - half ( j ) ;

half3 cf = half3 ( coefsHigh [ j ] , coefsHigh [ j + 1 ] , coefsHigh [ j + 2 ] ) ;
half3 monomials = half3 ( t * t , t , 1.0 ) ;
logy = dot ( monomials , mul ( M , cf ) ) ;
}
else
{
logy = logx * slopeHigh + ( log10 ( maxPoint . y ) - slopeHigh * log10 ( maxPoint . x ) ) ;
}

return pow ( 10.0 , logy ) ;
}

struct SegmentedSplineParams_c9
{
float coefsLow [ 10 ] ;
float coefsHigh [ 10 ] ;
half2 minPoint ;
half2 midPoint ;
half2 maxPoint ;
float slopeLow ;
float slopeHigh ;
} ;

half segmented_spline_c9_fwd ( half x , SegmentedSplineParams_c9 params )
{
const int N_KNOTS_LOW = 8 ;
const int N_KNOTS_HIGH = 8 ;
//#line 544
half xCheck = x ;
if ( xCheck <= 0.0 ) xCheck = 1e-4 ;

half logx = log10 ( xCheck ) ;
half logy ;

if ( logx <= log10 ( params . minPoint . x ) )
{
logy = logx * half ( params . slopeLow ) + ( log10 ( params . minPoint . y ) - half ( params . slopeLow ) * log10 ( params . minPoint . x ) ) ;
}
else if ( ( logx > log10 ( params . minPoint . x ) ) && ( logx < log10 ( params . midPoint . x ) ) )
{
half knot_coord = half ( N_KNOTS_LOW - 1 ) * ( logx - log10 ( params . minPoint . x ) ) / ( log10 ( params . midPoint . x ) - log10 ( params . minPoint . x ) ) ;
int j = knot_coord ;
half t = knot_coord - half ( j ) ;

half3 cf = half3 ( params . coefsLow [ j ] , params . coefsLow [ j + 1 ] , params . coefsLow [ j + 2 ] ) ;
half3 monomials = half3 ( t * t , t , 1.0 ) ;
logy = dot ( monomials , mul ( M , cf ) ) ;
}
else if ( ( logx >= log10 ( params . midPoint . x ) ) && ( logx < log10 ( params . maxPoint . x ) ) )
{
half knot_coord = half ( N_KNOTS_HIGH - 1 ) * ( logx - log10 ( params . midPoint . x ) ) / ( log10 ( params . maxPoint . x ) - log10 ( params . midPoint . x ) ) ;
int j = knot_coord ;
half t = knot_coord - half ( j ) ;

half3 cf = half3 ( params . coefsHigh [ j ] , params . coefsHigh [ j + 1 ] , params . coefsHigh [ j + 2 ] ) ;
half3 monomials = half3 ( t * t , t , 1.0 ) ;
logy = dot ( monomials , mul ( M , cf ) ) ;
}
else
{
logy = logx * half ( params . slopeHigh ) + ( log10 ( params . maxPoint . y ) - half ( params . slopeHigh ) * log10 ( params . maxPoint . x ) ) ;
}

return pow ( 10.0 , logy ) ;
}
//#line 584
SegmentedSplineParams_c9 GetSplineParams_ODT48Nits ( )
{
const SegmentedSplineParams_c9 ODT_48nits =
{

{ - 1.6989700043 , - 1.6989700043 , - 1.4779000000 , - 1.2291000000 , - 0.8648000000 , - 0.4480000000 , 0.0051800000 , 0.4511080334 , 0.9113744414 , 0.9113744414 } ,

{ 0.5154386965 , 0.8470437783 , 1.1358000000 , 1.3802000000 , 1.5197000000 , 1.5985000000 , 1.6467000000 , 1.6746091357 , 1.6878733390 , 1.6878733390 } ,
{ segmented_spline_c5_fwd ( 0.18 * pow ( 2. , - 6.5 ) ) , 0.02 } ,
{ segmented_spline_c5_fwd ( 0.18 ) , 4.8 } ,
{ segmented_spline_c5_fwd ( 0.18 * pow ( 2. , 6.5 ) ) , 48.0 } ,
0.0 ,
0.04
} ;
return ODT_48nits ;
}

SegmentedSplineParams_c9 GetSplineParams_ODT1000Nits ( )
{
const SegmentedSplineParams_c9 ODT_1000nits =
{

{ - 4.9706219331 , - 3.0293780669 , - 2.1262 , - 1.5105 , - 1.0578 , - 0.4668 , 0.11938 , 0.7088134201 , 1.2911865799 , 1.2911865799 } ,

{ 0.8089132070 , 1.1910867930 , 1.5683 , 1.9483 , 2.3083 , 2.6384 , 2.8595 , 2.9872608805 , 3.0127391195 , 3.0127391195 } ,
{ segmented_spline_c5_fwd ( 0.18 * pow ( 2. , - 12. ) ) , 0.0001 } ,
{ segmented_spline_c5_fwd ( 0.18 ) , 10.0 } ,
{ segmented_spline_c5_fwd ( 0.18 * pow ( 2. , 10. ) ) , 1000.0 } ,
3.0 ,
0.06
} ;
return ODT_1000nits ;
}

SegmentedSplineParams_c9 GetSplineParams_ODT2000Nits ( )
{
const SegmentedSplineParams_c9 ODT_2000nits =
{

{ - 4.9706219331 , - 3.0293780669 , - 2.1262 , - 1.5105 , - 1.0578 , - 0.4668 , 0.11938 , 0.7088134201 , 1.2911865799 , 1.2911865799 } ,

{ 0.8019952042 , 1.1980047958 , 1.5943000000 , 1.9973000000 , 2.3783000000 , 2.7684000000 , 3.0515000000 , 3.2746293562 , 3.3274306351 , 3.3274306351 } ,
{ segmented_spline_c5_fwd ( 0.18 * pow ( 2. , - 12. ) ) , 0.0001 } ,
{ segmented_spline_c5_fwd ( 0.18 ) , 10.0 } ,
{ segmented_spline_c5_fwd ( 0.18 * pow ( 2. , 11. ) ) , 2000.0 } ,
3.0 ,
0.12
} ;
return ODT_2000nits ;
}

SegmentedSplineParams_c9 GetSplineParams_ODT4000Nits ( )
{
const SegmentedSplineParams_c9 ODT_4000nits =
{

{ - 4.9706219331 , - 3.0293780669 , - 2.1262 , - 1.5105 , - 1.0578 , - 0.4668 , 0.11938 , 0.7088134201 , 1.2911865799 , 1.2911865799 } ,

{ 0.7973186613 , 1.2026813387 , 1.6093000000 , 2.0108000000 , 2.4148000000 , 2.8179000000 , 3.1725000000 , 3.5344995451 , 3.6696204376 , 3.6696204376 } ,
{ segmented_spline_c5_fwd ( 0.18 * pow ( 2. , - 12. ) ) , 0.0001 } ,
{ segmented_spline_c5_fwd ( 0.18 ) , 10.0 } ,
{ segmented_spline_c5_fwd ( 0.18 * pow ( 2. , 12. ) ) , 4000.0 } ,
3.0 ,
0.3
} ;
return ODT_4000nits ;
}
//#line 653
half segmented_spline_c9_fwd ( half x )
{
return segmented_spline_c9_fwd ( x , GetSplineParams_ODT48Nits ( ) ) ;
}

static const half RRT_GLOW_GAIN = 0.05 ;
static const half RRT_GLOW_MID = 0.08 ;

static const half RRT_RED_SCALE = 0.82 ;
static const half RRT_RED_PIVOT = 0.03 ;
static const half RRT_RED_HUE = 0.0 ;
static const half RRT_RED_WIDTH = 135.0 ;

static const half RRT_SAT_FACTOR = 0.96 ;

half3 RRT ( half3 aces )
{

half saturation = rgb_2_saturation ( aces ) ;
half ycIn = rgb_2_yc ( aces ) ;
half s = sigmoid_shaper ( ( saturation - 0.4 ) / 0.2 ) ;
half addedGlow = 1.0 + glow_fwd ( ycIn , RRT_GLOW_GAIN * s , RRT_GLOW_MID ) ;
aces *= addedGlow ;
//#line 678
half hue = rgb_2_hue ( aces ) ;
half centeredHue = center_hue ( hue , RRT_RED_HUE ) ;
half hueWeight ;
{

hueWeight = smoothstep ( 0.0 , 1.0 , 1.0 - abs ( 2.0 * centeredHue / RRT_RED_WIDTH ) ) ;
hueWeight *= hueWeight ;
}

aces . r += hueWeight * saturation * ( RRT_RED_PIVOT - aces . r ) * ( 1.0 - RRT_RED_SCALE ) ;
//#line 690
aces = clamp ( aces , 0.0 , 65504.0 ) ;
half3 rgbPre = mul ( AP0_2_AP1_MAT , aces ) ;
rgbPre = clamp ( rgbPre , 0 , 65504.0 ) ;
//#line 696
rgbPre = lerp ( dot ( rgbPre , AP1_RGB2Y ) . xxx , rgbPre , RRT_SAT_FACTOR . xxx ) ;
//#line 699
half3 rgbPost ;
rgbPost . x = segmented_spline_c5_fwd ( rgbPre . x ) ;
rgbPost . y = segmented_spline_c5_fwd ( rgbPre . y ) ;
rgbPost . z = segmented_spline_c5_fwd ( rgbPre . z ) ;
//#line 705
half3 outputVal = mul ( AP1_2_AP0_MAT , rgbPost ) ;

return outputVal ;
}
//#line 713
half3 Y_2_linCV ( half3 Y , half Ymax , half Ymin )
{
return ( Y - Ymin ) / ( Ymax - Ymin ) ;
}

half3 XYZ_2_xyY ( half3 XYZ )
{
half divisor = max ( dot ( XYZ , ( 1.0 ) . xxx ) , 1e-4 ) ;
return half3 ( XYZ . xy / divisor , XYZ . y ) ;
}

half3 xyY_2_XYZ ( half3 xyY )
{
half m = xyY . z / max ( xyY . y , 1e-4 ) ;
half3 XYZ = half3 ( xyY . xz , ( 1.0 - xyY . x - xyY . y ) ) ;
XYZ . xz *= m ;
return XYZ ;
}

static const half DIM_SURROUND_GAMMA = 0.9811 ;

half3 darkSurround_to_dimSurround ( half3 linearCV )
{

half3 XYZ = ( half3 ) mul ( AP1_2_XYZ_MAT , ( float3 ) linearCV ) ;

half3 xyY = XYZ_2_xyY ( XYZ ) ;
xyY . z = clamp ( xyY . z , 0.0 , 65504.0 ) ;
xyY . z = pow ( xyY . z , DIM_SURROUND_GAMMA ) ;
XYZ = xyY_2_XYZ ( xyY ) ;

return mul ( XYZ_2_AP1_MAT , XYZ ) ;
}

half moncurve_r ( half y , half gamma , half offs )
{

half x ;
const half yb = pow ( offs * gamma / ( ( gamma - 1.0 ) * ( 1.0 + offs ) ) , gamma ) ;
const half rs = pow ( ( gamma - 1.0 ) / offs , gamma - 1.0 ) * pow ( ( 1.0 + offs ) / gamma , gamma ) ;
if ( y >= yb )
x = ( 1.0 + offs ) * pow ( y , 1.0 / gamma ) - offs ;
else
x = y * rs ;
return x ;
}

half bt1886_r ( half L , half gamma , half Lw , half Lb )
{
//#line 764
half a = pow ( pow ( Lw , 1.0 / gamma ) - pow ( Lb , 1.0 / gamma ) , gamma ) ;
half b = pow ( Lb , 1.0 / gamma ) / ( pow ( Lw , 1.0 / gamma ) - pow ( Lb , 1.0 / gamma ) ) ;
half V = pow ( max ( L / a , 0.0 ) , 1.0 / gamma ) - b ;
return V ;
}

half roll_white_fwd (
half x ,
half new_wht ,
half width
)
{
const half x0 = - 1.0 ;
const half x1 = x0 + width ;
const half y0 = - new_wht ;
const half y1 = x1 ;
const half m1 = ( x1 - x0 ) ;
const half a = y0 - y1 + m1 ;
const half b = 2.0 * ( y1 - y0 ) - m1 ;
const half c = y0 ;
const half t = ( - x - x0 ) / ( x1 - x0 ) ;
half o = 0.0 ;
if ( t < 0.0 )
o = - ( t * b + c ) ;
else if ( t > 1.0 )
o = x ;
else
o = - ( ( t * a + b ) * t + c ) ;
return o ;
}

half3 linear_to_bt1886 ( half3 x , half gamma , half Lw , half Lb )
{
//#line 799
return pow ( max ( x , 0.0 ) , 1.0 / 2.4 ) ;
//#line 803
half invgamma = 1.0 / gamma ;
half p_Lw = pow ( Lw , invgamma ) ;
half p_Lb = pow ( Lb , invgamma ) ;
half3 a = pow ( p_Lw - p_Lb , gamma ) . xxx ;
half3 b = ( p_Lb / p_Lw - p_Lb ) . xxx ;
half3 V = pow ( max ( x / a , 0.0 ) , invgamma . xxx ) - b ;
return V ;
}

static const half CINEMA_WHITE = 48.0 ;
static const half CINEMA_BLACK = CINEMA_WHITE / 2400.0 ;
static const half ODT_SAT_FACTOR = 0.93 ;
//#line 860
half3 ODT_RGBmonitor_100nits_dim ( half3 oces )
{
const SegmentedSplineParams_c9 ODT_48nits = GetSplineParams_ODT48Nits ( ) ;
//#line 865
half3 rgbPre = mul ( AP0_2_AP1_MAT , oces ) ;
//#line 868
half3 rgbPost ;
rgbPost . x = segmented_spline_c9_fwd ( rgbPre . x , ODT_48nits ) ;
rgbPost . y = segmented_spline_c9_fwd ( rgbPre . y , ODT_48nits ) ;
rgbPost . z = segmented_spline_c9_fwd ( rgbPre . z , ODT_48nits ) ;
//#line 874
half3 linearCV = Y_2_linCV ( rgbPost , CINEMA_WHITE , CINEMA_BLACK ) ;
//#line 877
linearCV = darkSurround_to_dimSurround ( linearCV ) ;
//#line 881
linearCV = lerp ( dot ( linearCV , AP1_RGB2Y ) . xxx , linearCV , ODT_SAT_FACTOR . xxx ) ;
//#line 885
half3 XYZ = mul ( AP1_2_XYZ_MAT , linearCV ) ;
//#line 888
XYZ = mul ( D60_2_D65_CAT , XYZ ) ;
//#line 891
linearCV = mul ( XYZ_2_REC709_MAT , XYZ ) ;
//#line 895
linearCV = saturate ( linearCV ) ;
//#line 913
return linearCV ;
}
//#line 960
half3 ODT_RGBmonitor_D60sim_100nits_dim ( half3 oces )
{
const SegmentedSplineParams_c9 ODT_48nits = GetSplineParams_ODT48Nits ( ) ;
//#line 965
half3 rgbPre = mul ( AP0_2_AP1_MAT , oces ) ;
//#line 968
half3 rgbPost ;
rgbPost . x = segmented_spline_c9_fwd ( rgbPre . x , ODT_48nits ) ;
rgbPost . y = segmented_spline_c9_fwd ( rgbPre . y , ODT_48nits ) ;
rgbPost . z = segmented_spline_c9_fwd ( rgbPre . z , ODT_48nits ) ;
//#line 974
half3 linearCV = Y_2_linCV ( rgbPost , CINEMA_WHITE , CINEMA_BLACK ) ;
//#line 991
const half SCALE = 0.955 ;
linearCV = min ( linearCV , 1.0 ) * SCALE ;
//#line 995
linearCV = darkSurround_to_dimSurround ( linearCV ) ;
//#line 999
linearCV = lerp ( dot ( linearCV , AP1_RGB2Y ) . xxx , linearCV , ODT_SAT_FACTOR . xxx ) ;
//#line 1003
half3 XYZ = mul ( AP1_2_XYZ_MAT , linearCV ) ;
//#line 1006
linearCV = mul ( XYZ_2_REC709_MAT , XYZ ) ;
//#line 1010
linearCV = saturate ( linearCV ) ;
//#line 1028
return linearCV ;
}
//#line 1072
half3 ODT_Rec709_100nits_dim ( half3 oces )
{
const SegmentedSplineParams_c9 ODT_48nits = GetSplineParams_ODT48Nits ( ) ;
//#line 1077
half3 rgbPre = mul ( AP0_2_AP1_MAT , oces ) ;
//#line 1080
half3 rgbPost ;
rgbPost . x = segmented_spline_c9_fwd ( rgbPre . x , ODT_48nits ) ;
rgbPost . y = segmented_spline_c9_fwd ( rgbPre . y , ODT_48nits ) ;
rgbPost . z = segmented_spline_c9_fwd ( rgbPre . z , ODT_48nits ) ;
//#line 1086
half3 linearCV = Y_2_linCV ( rgbPost , CINEMA_WHITE , CINEMA_BLACK ) ;
//#line 1089
linearCV = darkSurround_to_dimSurround ( linearCV ) ;
//#line 1093
linearCV = lerp ( dot ( linearCV , AP1_RGB2Y ) . xxx , linearCV , ODT_SAT_FACTOR . xxx ) ;
//#line 1097
half3 XYZ = mul ( AP1_2_XYZ_MAT , linearCV ) ;
//#line 1100
XYZ = mul ( D60_2_D65_CAT , XYZ ) ;
//#line 1103
linearCV = mul ( XYZ_2_REC709_MAT , XYZ ) ;
//#line 1107
linearCV = saturate ( linearCV ) ;
//#line 1110
const half DISPGAMMA = 2.4 ;
const half L_W = 1.0 ;
const half L_B = 0.0 ;
half3 outputCV = linear_to_bt1886 ( linearCV , DISPGAMMA , L_W , L_B ) ;
//#line 1122
return outputCV ;
}
//#line 1166
half3 ODT_Rec709_D60sim_100nits_dim ( half3 oces )
{
const SegmentedSplineParams_c9 ODT_48nits = GetSplineParams_ODT48Nits ( ) ;
//#line 1171
half3 rgbPre = mul ( AP0_2_AP1_MAT , oces ) ;
//#line 1174
half3 rgbPost ;
rgbPost . x = segmented_spline_c9_fwd ( rgbPre . x , ODT_48nits ) ;
rgbPost . y = segmented_spline_c9_fwd ( rgbPre . y , ODT_48nits ) ;
rgbPost . z = segmented_spline_c9_fwd ( rgbPre . z , ODT_48nits ) ;
//#line 1180
half3 linearCV = Y_2_linCV ( rgbPost , CINEMA_WHITE , CINEMA_BLACK ) ;
//#line 1198
const half SCALE = 0.955 ;
linearCV = min ( linearCV , 1.0 ) * SCALE ;
//#line 1202
linearCV = darkSurround_to_dimSurround ( linearCV ) ;
//#line 1206
linearCV = lerp ( dot ( linearCV , AP1_RGB2Y ) . xxx , linearCV , ODT_SAT_FACTOR . xxx ) ;
//#line 1210
half3 XYZ = mul ( AP1_2_XYZ_MAT , linearCV ) ;
//#line 1213
linearCV = mul ( XYZ_2_REC709_MAT , XYZ ) ;
//#line 1217
linearCV = saturate ( linearCV ) ;
//#line 1220
const half DISPGAMMA = 2.4 ;
const half L_W = 1.0 ;
const half L_B = 0.0 ;
half3 outputCV = linear_to_bt1886 ( linearCV , DISPGAMMA , L_W , L_B ) ;
//#line 1232
return outputCV ;
}
//#line 1278
half3 ODT_Rec2020_100nits_dim ( half3 oces )
{
const SegmentedSplineParams_c9 ODT_48nits = GetSplineParams_ODT48Nits ( ) ;
//#line 1283
half3 rgbPre = mul ( AP0_2_AP1_MAT , oces ) ;
//#line 1286
half3 rgbPost ;
rgbPost . x = segmented_spline_c9_fwd ( rgbPre . x , ODT_48nits ) ;
rgbPost . y = segmented_spline_c9_fwd ( rgbPre . y , ODT_48nits ) ;
rgbPost . z = segmented_spline_c9_fwd ( rgbPre . z , ODT_48nits ) ;
//#line 1292
half3 linearCV = Y_2_linCV ( rgbPost , CINEMA_WHITE , CINEMA_BLACK ) ;
//#line 1295
linearCV = darkSurround_to_dimSurround ( linearCV ) ;
//#line 1299
linearCV = lerp ( dot ( linearCV , AP1_RGB2Y ) . xxx , linearCV , ODT_SAT_FACTOR . xxx ) ;
//#line 1303
half3 XYZ = mul ( AP1_2_XYZ_MAT , linearCV ) ;
//#line 1306
XYZ = mul ( D60_2_D65_CAT , XYZ ) ;
//#line 1309
linearCV = mul ( XYZ_2_REC2020_MAT , XYZ ) ;
//#line 1313
linearCV = saturate ( linearCV ) ;
//#line 1316
const half DISPGAMMA = 2.4 ;
const half L_W = 1.0 ;
const half L_B = 0.0 ;
half3 outputCV = linear_to_bt1886 ( linearCV , DISPGAMMA , L_W , L_B ) ;
//#line 1328
return outputCV ;
}
//#line 1362
half3 ODT_P3DCI_48nits ( half3 oces )
{
const SegmentedSplineParams_c9 ODT_48nits = GetSplineParams_ODT48Nits ( ) ;
//#line 1367
half3 rgbPre = mul ( AP0_2_AP1_MAT , oces ) ;
//#line 1370
half3 rgbPost ;
rgbPost . x = segmented_spline_c9_fwd ( rgbPre . x , ODT_48nits ) ;
rgbPost . y = segmented_spline_c9_fwd ( rgbPre . y , ODT_48nits ) ;
rgbPost . z = segmented_spline_c9_fwd ( rgbPre . z , ODT_48nits ) ;
//#line 1376
half3 linearCV = Y_2_linCV ( rgbPost , CINEMA_WHITE , CINEMA_BLACK ) ;
//#line 1399
const half NEW_WHT = 0.918 ;
const half ROLL_WIDTH = 0.5 ;
linearCV . x = roll_white_fwd ( linearCV . x , NEW_WHT , ROLL_WIDTH ) ;
linearCV . y = roll_white_fwd ( linearCV . y , NEW_WHT , ROLL_WIDTH ) ;
linearCV . z = roll_white_fwd ( linearCV . z , NEW_WHT , ROLL_WIDTH ) ;
//#line 1406
const half SCALE = 0.96 ;
linearCV = min ( linearCV , NEW_WHT ) * SCALE ;
//#line 1411
half3 XYZ = mul ( AP1_2_XYZ_MAT , linearCV ) ;
//#line 1414
linearCV = mul ( XYZ_2_DCIP3_MAT , XYZ ) ;
//#line 1418
linearCV = saturate ( linearCV ) ;
//#line 1421
const half DISPGAMMA = 2.6 ;
half3 outputCV = pow ( linearCV , 1.0 / DISPGAMMA ) ;
//#line 1429
return outputCV ;
}
//#line 1434
half3 ODT_Rec2020_1000nits_ToLinear ( half3 oces )
{
const SegmentedSplineParams_c9 ODT_1000nits = GetSplineParams_ODT1000Nits ( ) ;
//#line 1439
half3 rgbPre = mul ( AP0_2_AP1_MAT , oces ) ;
//#line 1442
half3 rgbPost ;
rgbPost . x = segmented_spline_c9_fwd ( rgbPre . x , ODT_1000nits ) ;
rgbPost . y = segmented_spline_c9_fwd ( rgbPre . y , ODT_1000nits ) ;
rgbPost . z = segmented_spline_c9_fwd ( rgbPre . z , ODT_1000nits ) ;
//#line 1448
half3 linearCV = Y_2_linCV ( rgbPost , ODT_1000nits . maxPoint . y , ODT_1000nits . minPoint . y ) ;
//#line 1452
linearCV = lerp ( dot ( linearCV , AP1_RGB2Y ) . xxx , linearCV , ODT_SAT_FACTOR . xxx ) ;
//#line 1456
half3 XYZ = mul ( AP1_2_XYZ_MAT , linearCV ) ;
//#line 1459
XYZ = mul ( D60_2_D65_CAT , XYZ ) ;
//#line 1462
linearCV = mul ( XYZ_2_REC2020_MAT , XYZ ) ;
//#line 1465
linearCV = max ( linearCV , 0. ) ;

return linearCV ;
}

half3 ODT_1000nits_ToAP1 ( half3 oces )
{
const SegmentedSplineParams_c9 ODT_1000nits = GetSplineParams_ODT1000Nits ( ) ;
//#line 1475
half3 rgbPre = mul ( AP0_2_AP1_MAT , oces ) ;
//#line 1478
half3 rgbPost ;
rgbPost . x = segmented_spline_c9_fwd ( rgbPre . x , ODT_1000nits ) ;
rgbPost . y = segmented_spline_c9_fwd ( rgbPre . y , ODT_1000nits ) ;
rgbPost . z = segmented_spline_c9_fwd ( rgbPre . z , ODT_1000nits ) ;

return rgbPost ;
}

half3 ODT_2000nits_ToAP1 ( half3 oces )
{
const SegmentedSplineParams_c9 ODT_2000nits = GetSplineParams_ODT2000Nits ( ) ;
//#line 1491
half3 rgbPre = mul ( AP0_2_AP1_MAT , oces ) ;
//#line 1494
half3 rgbPost ;
rgbPost . x = segmented_spline_c9_fwd ( rgbPre . x , ODT_2000nits ) ;
rgbPost . y = segmented_spline_c9_fwd ( rgbPre . y , ODT_2000nits ) ;
rgbPost . z = segmented_spline_c9_fwd ( rgbPre . z , ODT_2000nits ) ;

return rgbPost ;
}

half3 ODT_4000nits_ToAP1 ( half3 oces )
{
const SegmentedSplineParams_c9 ODT_4000nits = GetSplineParams_ODT4000Nits ( ) ;
//#line 1507
half3 rgbPre = mul ( AP0_2_AP1_MAT , oces ) ;
//#line 1510
half3 rgbPost ;
rgbPost . x = segmented_spline_c9_fwd ( rgbPre . x , ODT_4000nits ) ;
rgbPost . y = segmented_spline_c9_fwd ( rgbPre . y , ODT_4000nits ) ;
rgbPost . z = segmented_spline_c9_fwd ( rgbPre . z , ODT_4000nits ) ;

return rgbPost ;
}
//#line 15 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
float Gamma20ToLinear ( float c )
{
return c * c ;
}

float3 Gamma20ToLinear ( float3 c )
{
return c . rgb * c . rgb ;
}

float4 Gamma20ToLinear ( float4 c )
{
return float4 ( Gamma20ToLinear ( c . rgb ) , c . a ) ;
}

float LinearToGamma20 ( float c )
{
return sqrt ( c ) ;
}

float3 LinearToGamma20 ( float3 c )
{
return sqrt ( c . rgb ) ;
}

float4 LinearToGamma20 ( float4 c )
{
return float4 ( LinearToGamma20 ( c . rgb ) , c . a ) ;
}
//#line 46
float Gamma22ToLinear ( float c )
{
return PositivePow ( c , float ( 2.2 ) ) ;
}

float3 Gamma22ToLinear ( float3 c )
{
return PositivePow ( c . rgb , float3 ( 2.2 , 2.2 , 2.2 ) ) ;
}

float4 Gamma22ToLinear ( float4 c )
{
return float4 ( Gamma22ToLinear ( c . rgb ) , c . a ) ;
}

float LinearToGamma22 ( float c )
{
return PositivePow ( c , float ( 0.454545454545455 ) ) ;
}

float3 LinearToGamma22 ( float3 c )
{
return PositivePow ( c . rgb , float3 ( 0.454545454545455 , 0.454545454545455 , 0.454545454545455 ) ) ;
}

float4 LinearToGamma22 ( float4 c )
{
return float4 ( LinearToGamma22 ( c . rgb ) , c . a ) ;
}
//#line 77
float SRGBToLinear ( float c )
{
//#line 82
float linearRGBLo = c / 12.92 ;
float linearRGBHi = PositivePow ( ( c + 0.055 ) / 1.055 , float ( 2.4 ) ) ;
float linearRGB = ( c <= 0.04045 ) ? linearRGBLo : linearRGBHi ;
return linearRGB ;
}

float2 SRGBToLinear ( float2 c )
{
return float2 ( SRGBToLinear ( c . r ) , SRGBToLinear ( c . g ) ) ;
}

float3 SRGBToLinear ( float3 c )
{
return float3 ( SRGBToLinear ( c . r ) , SRGBToLinear ( c . g ) , SRGBToLinear ( c . b ) ) ;
}

float4 SRGBToLinear ( float4 c )
{
return float4 ( SRGBToLinear ( c . r ) , SRGBToLinear ( c . g ) , SRGBToLinear ( c . b ) , c . a ) ;
}

float LinearToSRGB ( float c )
{
return ( c <= 0.0031308 ) ? ( c * 12.9232102 ) : 1.055 * PositivePow ( c , 1.0 / 2.4 ) - 0.055 ;
}

float2 LinearToSRGB ( float2 c )
{
return float2 ( LinearToSRGB ( c . r ) , LinearToSRGB ( c . g ) ) ;
}

float3 LinearToSRGB ( float3 c )
{
return float3 ( LinearToSRGB ( c . r ) , LinearToSRGB ( c . g ) , LinearToSRGB ( c . b ) ) ;
}

float4 LinearToSRGB ( float4 c )
{
return float4 ( LinearToSRGB ( c . r ) , LinearToSRGB ( c . g ) , LinearToSRGB ( c . b ) , c . a ) ;
}
//#line 125
float FastSRGBToLinear ( float c )
{
return c * ( c * ( c * 0.305306011 + 0.682171111 ) + 0.012522878 ) ;
}

float2 FastSRGBToLinear ( float2 c )
{
return c * ( c * ( c * 0.305306011 + 0.682171111 ) + 0.012522878 ) ;
}

float3 FastSRGBToLinear ( float3 c )
{
return c * ( c * ( c * 0.305306011 + 0.682171111 ) + 0.012522878 ) ;
}

float4 FastSRGBToLinear ( float4 c )
{
return float4 ( FastSRGBToLinear ( c . rgb ) , c . a ) ;
}

float FastLinearToSRGB ( float c )
{
return saturate ( 1.055 * PositivePow ( c , float ( 0.416666667 ) ) - 0.055 ) ;
}

float2 FastLinearToSRGB ( float2 c )
{
return saturate ( 1.055 * PositivePow ( c , float ( 0.416666667 ) ) - 0.055 ) ;
}

float3 FastLinearToSRGB ( float3 c )
{
return saturate ( 1.055 * PositivePow ( c , float ( 0.416666667 ) ) - 0.055 ) ;
}

float4 FastLinearToSRGB ( float4 c )
{
return float4 ( FastLinearToSRGB ( c . rgb ) , c . a ) ;
}
//#line 172
float Luminance ( float3 linearRgb )
{
return dot ( linearRgb , float3 ( 0.2126729 , 0.7151522 , 0.0721750 ) ) ;
}
//#line 178
float Luminance ( float4 linearRgba )
{
return Luminance ( linearRgba . rgb ) ;
}

float AcesLuminance ( float3 linearRgb )
{
return dot ( linearRgb , AP1_RGB2Y ) ;
}

float AcesLuminance ( float4 linearRgba )
{
return AcesLuminance ( linearRgba . rgb ) ;
}
//#line 197
float ScotopicLuminance ( float3 xyzRgb )
{
float X = xyzRgb . x ;
float Y = xyzRgb . y ;
float Z = xyzRgb . z ;
return Y * ( 1.33 * ( 1.0 + ( Y + Z ) / X ) - 1.68 ) ;
}

float ScotopicLuminance ( float4 xyzRgba )
{
return ScotopicLuminance ( xyzRgba . rgb ) ;
}
//#line 214
float3 RGBToYCoCg ( float3 rgb )
{
float3 YCoCg ;
YCoCg . x = dot ( rgb , float3 ( 0.25 , 0.5 , 0.25 ) ) ;
YCoCg . y = dot ( rgb , float3 ( 0.5 , 0.0 , - 0.5 ) ) + ( 128.0 / 255.0 ) ;
YCoCg . z = dot ( rgb , float3 ( - 0.25 , 0.5 , - 0.25 ) ) + ( 128.0 / 255.0 ) ;

return YCoCg ;
}

float3 YCoCgToRGB ( float3 YCoCg )
{
float Y = YCoCg . x ;
float Co = YCoCg . y - ( 128.0 / 255.0 ) ;
float Cg = YCoCg . z - ( 128.0 / 255.0 ) ;

float3 rgb ;
rgb . r = Y + Co - Cg ;
rgb . g = Y + Cg ;
rgb . b = Y - Co - Cg ;

return rgb ;
}
//#line 240
float YCoCgCheckBoardEdgeFilter ( float centerLum , float2 a0 , float2 a1 , float2 a2 , float2 a3 )
{
float4 lum = float4 ( a0 . x , a1 . x , a2 . x , a3 . x ) ;

float4 w = 1.0 - saturate ( ( abs ( lum . xxxx - centerLum ) - 30.0 / 255.0 ) * 65504.0 ) ;
float W = w . x + w . y + w . z + w . w ;

return ( W == 0.0 ) ? a0 . y : ( w . x * a0 . y + w . y * a1 . y + w . z * a2 . y + w . w * a3 . y ) / W ;
}
//#line 252
float3 LinearToLMS ( float3 x )
{
const float3x3 LIN_2_LMS_MAT = {
3.90405e-1 , 5.49941e-1 , 8.92632e-3 ,
7.08416e-2 , 9.63172e-1 , 1.35775e-3 ,
2.31082e-2 , 1.28021e-1 , 9.36245e-1
} ;

return mul ( LIN_2_LMS_MAT , x ) ;
}
//#line 264
float3 LMSToLinear ( float3 x )
{
const float3x3 LMS_2_LIN_MAT = {
2.85847e+0 , - 1.62879e+0 , - 2.48910e-2 ,
- 2.10182e-1 , 1.15820e+0 , 3.24281e-4 ,
- 4.18120e-2 , - 1.18169e-1 , 1.06867e+0
} ;

return mul ( LMS_2_LIN_MAT , x ) ;
}
//#line 280
float3 RgbToHsv ( float3 c )
{
const float4 K = float4 ( 0.0 , - 1.0 / 3.0 , 2.0 / 3.0 , - 1.0 ) ;
float4 p = lerp ( float4 ( c . bg , K . wz ) , float4 ( c . gb , K . xy ) , step ( c . b , c . g ) ) ;
float4 q = lerp ( float4 ( p . xyw , c . r ) , float4 ( c . r , p . yzx ) , step ( p . x , c . r ) ) ;
float d = q . x - min ( q . w , q . y ) ;
const float e = 1.0e-4 ;
return float3 ( abs ( q . z + ( q . w - q . y ) / ( 6.0 * d + e ) ) , d / ( q . x + e ) , q . x ) ;
}

float3 HsvToRgb ( float3 c )
{
const float4 K = float4 ( 1.0 , 2.0 / 3.0 , 1.0 / 3.0 , 3.0 ) ;
float3 p = abs ( frac ( c . xxx + K . xyz ) * 6.0 - K . www ) ;
return c . z * lerp ( K . xxx , saturate ( p - K . xxx ) , c . y ) ;
}

float RotateHue ( float value , float low , float hi )
{
return ( value < low )
? value + hi
: ( value > hi )
? value - hi
: value ;
}
//#line 307
float3 xyYtoXYZ ( float3 xyY )
{
float x = xyY . x ;
float y = xyY . y ;
float Y = xyY . z ;

float X = ( Y / y ) * x ;
float Z = ( Y / y ) * ( 1.0 - x - y ) ;

return float3 ( X , Y , Z ) ;
}
//#line 320
float2 XYZtoxy ( float3 XYZ )
{
return XYZ . xy / ( dot ( XYZ , 1 ) ) ;
}
//#line 327
float3 SoftLight ( float3 base , float3 blend )
{
float3 r1 = 2.0 * base * blend + base * base * ( 1.0 - 2.0 * blend ) ;
float3 r2 = sqrt ( base ) * ( 2.0 * blend - 1.0 ) + 2.0 * base * ( 1.0 - blend ) ;
float3 t = step ( 0.5 , blend ) ;
return r2 * t + ( 1.0 - t ) * r1 ;
}
//#line 343
struct ParamsLogC
{
float cut ;
float a , b , c , d , e , f ;
} ;

static const ParamsLogC LogC =
{
0.011361 ,
5.555556 ,
0.047996 ,
0.244161 ,
0.386036 ,
5.301883 ,
0.092819
} ;

float LinearToLogC_Precise ( float x )
{
float o ;
if ( x > LogC . cut )
o = LogC . c * log10 ( max ( LogC . a * x + LogC . b , 0.0 ) ) + LogC . d ;
else
o = LogC . e * x + LogC . f ;
return o ;
}
//#line 371
float3 LinearToLogC ( float3 x )
{
//#line 380
return LogC . c * log10 ( max ( LogC . a * x + LogC . b , 0.0 ) ) + LogC . d ;

}

float LogCToLinear_Precise ( float x )
{
float o ;
if ( x > LogC . e * LogC . cut + LogC . f )
o = ( pow ( 10.0 , ( x - LogC . d ) / LogC . c ) - LogC . b ) / LogC . a ;
else
o = ( x - LogC . f ) / LogC . e ;
return o ;
}
//#line 395
float3 LogCToLinear ( float3 x )
{
//#line 404
return ( pow ( 10.0 , ( x - LogC . d ) / LogC . c ) - LogC . b ) / LogC . a ;

}
//#line 412
float3 Desaturate ( float3 value , float saturation )
{
//#line 416
float mean = Avg3 ( value . r , value . g , value . b ) ;
float3 dev = value - mean ;

return mean + dev * saturation ;
}
//#line 424
float FastTonemapPerChannel ( float c )
{
return c * rcp ( c + 1.0 ) ;
}

float2 FastTonemapPerChannel ( float2 c )
{
return c * rcp ( c + 1.0 ) ;
}

float3 FastTonemap ( float3 c )
{
return c * rcp ( Max3 ( c . r , c . g , c . b ) + 1.0 ) ;
}

float4 FastTonemap ( float4 c )
{
return float4 ( FastTonemap ( c . rgb ) , c . a ) ;
}

float3 FastTonemap ( float3 c , float w )
{
return c * ( w * rcp ( Max3 ( c . r , c . g , c . b ) + 1.0 ) ) ;
}

float4 FastTonemap ( float4 c , float w )
{
return float4 ( FastTonemap ( c . rgb , w ) , c . a ) ;
}

float FastTonemapPerChannelInvert ( float c )
{
return c * rcp ( 1.0 - c ) ;
}

float2 FastTonemapPerChannelInvert ( float2 c )
{
return c * rcp ( 1.0 - c ) ;
}

float3 FastTonemapInvert ( float3 c )
{
return c * rcp ( 1.0 - Max3 ( c . r , c . g , c . b ) ) ;
}

float4 FastTonemapInvert ( float4 c )
{
return float4 ( FastTonemapInvert ( c . rgb ) , c . a ) ;
}
//#line 476
float3 ApplyLut3D ( Texture3D tex , SamplerState samplerTex , float3 uvw , float2 scaleOffset )
{
uvw . xyz = uvw . xyz * scaleOffset . yyy * scaleOffset . xxx + scaleOffset . xxx * 0.5 ;
return tex . SampleLevel ( samplerTex , uvw , 0.0 ) . rgb ;
}
//#line 484
float3 ApplyLut2D ( Texture2D tex , SamplerState samplerTex , float3 uvw , float3 scaleOffset )
{

uvw . z *= scaleOffset . z ;
float shift = floor ( uvw . z ) ;
uvw . xy = uvw . xy * scaleOffset . z * scaleOffset . xy + scaleOffset . xy * 0.5 ;
uvw . x += shift * scaleOffset . y ;
uvw . xyz = lerp (
tex . SampleLevel ( samplerTex , uvw . xy , 0.0 ) . rgb ,
tex . SampleLevel ( samplerTex , uvw . xy + float2 ( scaleOffset . y , 0.0 ) , 0.0 ) . rgb ,
uvw . z - shift
) ;
return uvw ;
}
//#line 501
float3 GetLutStripValue ( float2 uv , float4 params )
{

uv . x *= params . x ;
//#line 507
float lutBase = floor ( uv . x ) ;
uv . x -= lutBase ;
//#line 511
float lutBaseU = lutBase * 2.0 * params . z ;
//#line 514
float3 color ;
color . rg = uv - params . zz ;
color . b = lutBaseU ;
//#line 519
return color * params . w ;
}
//#line 525
float3 NeutralCurve ( float3 x , float a , float b , float c , float d , float e , float f )
{
return float3 ( ( ( x * ( a * x + c * b ) + d * e ) / ( x * ( a * x + b ) + d * f ) ) - e / f ) ;
}
//#line 535
float3 NeutralTonemap ( float3 x )
{

const float a = 0.2 ;
const float b = 0.29 ;
const float c = 0.24 ;
const float d = 0.272 ;
const float e = 0.02 ;
const float f = 0.3 ;
const float whiteLevel = 5.3 ;
const float whiteClip = 1.0 ;
//#line 551
float3 whiteScale = ( 1.0 ) . xxx / NeutralCurve ( whiteLevel , a , b , c , d , e , f ) ;
x = NeutralCurve ( x * whiteScale , a , b , c , d , e , f ) ;
x *= whiteScale ;
//#line 556
x /= whiteClip . xxx ;

return x ;
}
//#line 563
float EvalCustomSegment ( float x , float4 segmentA , float2 segmentB )
{
const float kOffsetX = segmentA . x ;
const float kOffsetY = segmentA . y ;
const float kScaleX = segmentA . z ;
const float kScaleY = segmentA . w ;
const float kLnA = segmentB . x ;
const float kB = segmentB . y ;

float x0 = ( x - kOffsetX ) * kScaleX ;
float y0 = ( x0 > 0.0 ) ? exp ( kLnA + kB * log ( x0 ) ) : 0.0 ;
return y0 * kScaleY + kOffsetY ;
}

float EvalCustomCurve ( float x , float3 curve , float4 toeSegmentA , float2 toeSegmentB , float4 midSegmentA , float2 midSegmentB , float4 shoSegmentA , float2 shoSegmentB )
{
float4 segmentA ;
float2 segmentB ;

if ( x < curve . y )
{
segmentA = toeSegmentA ;
segmentB = toeSegmentB ;
}
else if ( x < curve . z )
{
segmentA = midSegmentA ;
segmentB = midSegmentB ;
}
else
{
segmentA = shoSegmentA ;
segmentB = shoSegmentB ;
}

return EvalCustomSegment ( x , segmentA , segmentB ) ;
}
//#line 602
float3 CustomTonemap ( float3 x , float3 curve , float4 toeSegmentA , float2 toeSegmentB , float4 midSegmentA , float2 midSegmentB , float4 shoSegmentA , float2 shoSegmentB )
{
float3 normX = x * curve . x ;
float3 ret ;
ret . x = EvalCustomCurve ( normX . x , curve , toeSegmentA , toeSegmentB , midSegmentA , midSegmentB , shoSegmentA , shoSegmentB ) ;
ret . y = EvalCustomCurve ( normX . y , curve , toeSegmentA , toeSegmentB , midSegmentA , midSegmentB , shoSegmentA , shoSegmentB ) ;
ret . z = EvalCustomCurve ( normX . z , curve , toeSegmentA , toeSegmentB , midSegmentA , midSegmentB , shoSegmentA , shoSegmentB ) ;
return ret ;
}
//#line 614
float3 InvertibleTonemap ( float3 x )
{
float y = rcp ( float ( 8.0f ) + Max3 ( x . r , x . g , x . b ) ) ;
return saturate ( x * float ( y ) ) ;
}

float3 InvertibleTonemapInverse ( float3 x )
{
float y = rcp ( max ( float ( 1.0 / 32768.0 ) , saturate ( float ( 1.0 / 8.0f ) - Max3 ( x . r , x . g , x . b ) * float ( 1.0 / 8.0f ) ) ) ) ;
return x * y ;
}
//#line 632
float3 AcesTonemap ( float3 aces )
{
//#line 643
half saturation = rgb_2_saturation ( half3 ( aces ) ) ;
half ycIn = rgb_2_yc ( half3 ( aces ) ) ;
half s = sigmoid_shaper ( ( saturation - 0.4 ) / 0.2 ) ;
float addedGlow = 1.0 + glow_fwd ( ycIn , RRT_GLOW_GAIN * s , RRT_GLOW_MID ) ;
aces *= addedGlow ;
//#line 650
half hue = rgb_2_hue ( half3 ( aces ) ) ;
half centeredHue = center_hue ( hue , RRT_RED_HUE ) ;
float hueWeight ;
{

hueWeight = smoothstep ( 0.0 , 1.0 , 1.0 - abs ( 2.0 * centeredHue / RRT_RED_WIDTH ) ) ;
hueWeight *= hueWeight ;
}

aces . r += hueWeight * saturation * ( RRT_RED_PIVOT - aces . r ) * ( 1.0 - RRT_RED_SCALE ) ;
//#line 662
float3 acescg = max ( 0.0 , ACES_to_ACEScg ( aces ) ) ;
//#line 666
acescg = lerp ( dot ( acescg , AP1_RGB2Y ) . xxx , acescg , RRT_SAT_FACTOR . xxx ) ;
//#line 670
const float a = 0.0245786f ;
const float b = 0.000090537f ;
const float c = 0.983729f ;
const float d = 0.4329510f ;
const float e = 0.238081f ;
//#line 684
float3 rgbPost = ( acescg * ( acescg + a ) - b ) /
( acescg * ( c * acescg + d ) + e ) ;
//#line 692
float3 linearCV = darkSurround_to_dimSurround ( half3 ( rgbPost ) ) ;
//#line 696
linearCV = lerp ( dot ( linearCV , AP1_RGB2Y ) . xxx , linearCV , ODT_SAT_FACTOR . xxx ) ;
//#line 700
float3 XYZ = mul ( AP1_2_XYZ_MAT , linearCV ) ;
//#line 703
XYZ = mul ( D60_2_D65_CAT , XYZ ) ;
//#line 706
linearCV = mul ( XYZ_2_REC709_MAT , XYZ ) ;

return linearCV ;
//#line 711
}
//#line 714
static const half kRGBMRange = 8.0 ;

half4 EncodeRGBM ( half3 color )
{
color *= 1.0 / kRGBMRange ;
half m = max ( max ( color . x , color . y ) , max ( color . z , 1e-5 ) ) ;
m = ceil ( m * 255 ) / 255 ;
return half4 ( color / m , m ) ;
}

half3 DecodeRGBM ( half4 rgbm )
{
return rgbm . xyz * rgbm . w * kRGBMRange ;
}
//#line 20 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/BSDF.hlsl"
struct CBSDF
{
float3 diffR ;
float3 specR ;
float3 diffT ;
float3 specT ;
} ;
//#line 32
float F_Schlick ( float f0 , float f90 , float u )
{
float x = 1.0 - u ;
float x2 = x * x ;
float x5 = x * x2 * x2 ;
return ( f90 - f0 ) * x5 + f0 ;
}

float F_Schlick ( float f0 , float u )
{
return F_Schlick ( f0 , 1.0 , u ) ;
}

float3 F_Schlick ( float3 f0 , float f90 , float u )
{
float x = 1.0 - u ;
float x2 = x * x ;
float x5 = x * x2 * x2 ;
return f0 * ( 1.0 - x5 ) + ( f90 * x5 ) ;
}

float3 F_Schlick ( float3 f0 , float u )
{
return F_Schlick ( f0 , 1.0 , u ) ;
}
//#line 59
float F_Transm_Schlick ( float f0 , float f90 , float u )
{
float x = 1.0 - u ;
float x2 = x * x ;
float x5 = x * x2 * x2 ;
return ( 1.0 - f90 * x5 ) - f0 * ( 1.0 - x5 ) ;
}
//#line 68
float F_Transm_Schlick ( float f0 , float u )
{
return F_Transm_Schlick ( f0 , 1.0 , u ) ;
}
//#line 74
float3 F_Transm_Schlick ( float3 f0 , float f90 , float u )
{
float x = 1.0 - u ;
float x2 = x * x ;
float x5 = x * x2 * x2 ;
return ( 1.0 - f90 * x5 ) - f0 * ( 1.0 - x5 ) ;
}
//#line 83
float3 F_Transm_Schlick ( float3 f0 , float u )
{
return F_Transm_Schlick ( f0 , 1.0 , u ) ;
}
//#line 91
float CosCriticalAngle ( float eta )
{
return sqrt ( max ( 1.0 - Sq ( eta ) , 0.0 ) ) ;
//#line 100
}
//#line 104
float F_FresnelDielectric ( float ior , float u )
{
float g = sqrt ( Sq ( ior ) + Sq ( u ) - 1.0 ) ;
//#line 109
return 1.0 - saturate ( 1.0 - 0.5 * Sq ( ( g - u ) / ( g + u ) ) * ( 1.0 + Sq ( ( ( g + u ) * u - 1.0 ) / ( ( g - u ) * u + 1.0 ) ) ) ) ;
}
//#line 115
float3 F_FresnelConductor ( float3 eta , float3 etak2 , float cosTheta )
{
float cosTheta2 = cosTheta * cosTheta ;
float sinTheta2 = 1.0 - cosTheta2 ;
float3 eta2 = eta * eta ;

float3 t0 = eta2 - etak2 - sinTheta2 ;
float3 a2plusb2 = sqrt ( t0 * t0 + 4.0 * eta2 * etak2 ) ;
float3 t1 = a2plusb2 + cosTheta2 ;
float3 a = sqrt ( 0.5 * ( a2plusb2 + t0 ) ) ;
float3 t2 = 2.0 * a * cosTheta ;
float3 Rs = ( t1 - t2 ) / ( t1 + t2 ) ;

float3 t3 = cosTheta2 * a2plusb2 + sinTheta2 * sinTheta2 ;
float3 t4 = t2 * sinTheta2 ;
float3 Rp = Rs * ( t3 - t4 ) / ( t3 + t4 ) ;

return 0.5 * ( Rp + Rs ) ;
}
//#line 137
min16float IorToFresnel0 ( min16float transmittedIor , min16float incidentIor ) { return Sq ( ( transmittedIor - incidentIor ) / ( transmittedIor + incidentIor ) ) ; } min16float2 IorToFresnel0 ( min16float2 transmittedIor , min16float2 incidentIor ) { return Sq ( ( transmittedIor - incidentIor ) / ( transmittedIor + incidentIor ) ) ; } min16float3 IorToFresnel0 ( min16float3 transmittedIor , min16float3 incidentIor ) { return Sq ( ( transmittedIor - incidentIor ) / ( transmittedIor + incidentIor ) ) ; } min16float4 IorToFresnel0 ( min16float4 transmittedIor , min16float4 incidentIor ) { return Sq ( ( transmittedIor - incidentIor ) / ( transmittedIor + incidentIor ) ) ; } float IorToFresnel0 ( float transmittedIor , float incidentIor ) { return Sq ( ( transmittedIor - incidentIor ) / ( transmittedIor + incidentIor ) ) ; } float2 IorToFresnel0 ( float2 transmittedIor , float2 incidentIor ) { return Sq ( ( transmittedIor - incidentIor ) / ( transmittedIor + incidentIor ) ) ; } float3 IorToFresnel0 ( float3 transmittedIor , float3 incidentIor ) { return Sq ( ( transmittedIor - incidentIor ) / ( transmittedIor + incidentIor ) ) ; } float4 IorToFresnel0 ( float4 transmittedIor , float4 incidentIor ) { return Sq ( ( transmittedIor - incidentIor ) / ( transmittedIor + incidentIor ) ) ; }

float IorToFresnel0 ( float transmittedIor )
{
return IorToFresnel0 ( transmittedIor , 1.0 ) ;
}
//#line 151
min16float Fresnel0ToIor ( min16float fresnel0 ) { return ( ( 1.0 + sqrt ( fresnel0 ) ) / ( 1.0 - sqrt ( fresnel0 ) ) ) ; } min16float2 Fresnel0ToIor ( min16float2 fresnel0 ) { return ( ( 1.0 + sqrt ( fresnel0 ) ) / ( 1.0 - sqrt ( fresnel0 ) ) ) ; } min16float3 Fresnel0ToIor ( min16float3 fresnel0 ) { return ( ( 1.0 + sqrt ( fresnel0 ) ) / ( 1.0 - sqrt ( fresnel0 ) ) ) ; } min16float4 Fresnel0ToIor ( min16float4 fresnel0 ) { return ( ( 1.0 + sqrt ( fresnel0 ) ) / ( 1.0 - sqrt ( fresnel0 ) ) ) ; } float Fresnel0ToIor ( float fresnel0 ) { return ( ( 1.0 + sqrt ( fresnel0 ) ) / ( 1.0 - sqrt ( fresnel0 ) ) ) ; } float2 Fresnel0ToIor ( float2 fresnel0 ) { return ( ( 1.0 + sqrt ( fresnel0 ) ) / ( 1.0 - sqrt ( fresnel0 ) ) ) ; } float3 Fresnel0ToIor ( float3 fresnel0 ) { return ( ( 1.0 + sqrt ( fresnel0 ) ) / ( 1.0 - sqrt ( fresnel0 ) ) ) ; } float4 Fresnel0ToIor ( float4 fresnel0 ) { return ( ( 1.0 + sqrt ( fresnel0 ) ) / ( 1.0 - sqrt ( fresnel0 ) ) ) ; }
//#line 159
min16float ConvertF0ForAirInterfaceToF0ForClearCoat15 ( min16float fresnel0 ) { return saturate ( - 0.0256868 + fresnel0 * ( 0.326846 + ( 0.978946 - 0.283835 * fresnel0 ) * fresnel0 ) ) ; } min16float2 ConvertF0ForAirInterfaceToF0ForClearCoat15 ( min16float2 fresnel0 ) { return saturate ( - 0.0256868 + fresnel0 * ( 0.326846 + ( 0.978946 - 0.283835 * fresnel0 ) * fresnel0 ) ) ; } min16float3 ConvertF0ForAirInterfaceToF0ForClearCoat15 ( min16float3 fresnel0 ) { return saturate ( - 0.0256868 + fresnel0 * ( 0.326846 + ( 0.978946 - 0.283835 * fresnel0 ) * fresnel0 ) ) ; } min16float4 ConvertF0ForAirInterfaceToF0ForClearCoat15 ( min16float4 fresnel0 ) { return saturate ( - 0.0256868 + fresnel0 * ( 0.326846 + ( 0.978946 - 0.283835 * fresnel0 ) * fresnel0 ) ) ; } float ConvertF0ForAirInterfaceToF0ForClearCoat15 ( float fresnel0 ) { return saturate ( - 0.0256868 + fresnel0 * ( 0.326846 + ( 0.978946 - 0.283835 * fresnel0 ) * fresnel0 ) ) ; } float2 ConvertF0ForAirInterfaceToF0ForClearCoat15 ( float2 fresnel0 ) { return saturate ( - 0.0256868 + fresnel0 * ( 0.326846 + ( 0.978946 - 0.283835 * fresnel0 ) * fresnel0 ) ) ; } float3 ConvertF0ForAirInterfaceToF0ForClearCoat15 ( float3 fresnel0 ) { return saturate ( - 0.0256868 + fresnel0 * ( 0.326846 + ( 0.978946 - 0.283835 * fresnel0 ) * fresnel0 ) ) ; } float4 ConvertF0ForAirInterfaceToF0ForClearCoat15 ( float4 fresnel0 ) { return saturate ( - 0.0256868 + fresnel0 * ( 0.326846 + ( 0.978946 - 0.283835 * fresnel0 ) * fresnel0 ) ) ; }
//#line 162
min16float ConvertF0ForAirInterfaceToF0ForClearCoat15Fast ( min16float fresnel0 ) { return saturate ( fresnel0 * ( fresnel0 * 0.526868 + 0.529324 ) - 0.0482256 ) ; } min16float2 ConvertF0ForAirInterfaceToF0ForClearCoat15Fast ( min16float2 fresnel0 ) { return saturate ( fresnel0 * ( fresnel0 * 0.526868 + 0.529324 ) - 0.0482256 ) ; } min16float3 ConvertF0ForAirInterfaceToF0ForClearCoat15Fast ( min16float3 fresnel0 ) { return saturate ( fresnel0 * ( fresnel0 * 0.526868 + 0.529324 ) - 0.0482256 ) ; } min16float4 ConvertF0ForAirInterfaceToF0ForClearCoat15Fast ( min16float4 fresnel0 ) { return saturate ( fresnel0 * ( fresnel0 * 0.526868 + 0.529324 ) - 0.0482256 ) ; } float ConvertF0ForAirInterfaceToF0ForClearCoat15Fast ( float fresnel0 ) { return saturate ( fresnel0 * ( fresnel0 * 0.526868 + 0.529324 ) - 0.0482256 ) ; } float2 ConvertF0ForAirInterfaceToF0ForClearCoat15Fast ( float2 fresnel0 ) { return saturate ( fresnel0 * ( fresnel0 * 0.526868 + 0.529324 ) - 0.0482256 ) ; } float3 ConvertF0ForAirInterfaceToF0ForClearCoat15Fast ( float3 fresnel0 ) { return saturate ( fresnel0 * ( fresnel0 * 0.526868 + 0.529324 ) - 0.0482256 ) ; } float4 ConvertF0ForAirInterfaceToF0ForClearCoat15Fast ( float4 fresnel0 ) { return saturate ( fresnel0 * ( fresnel0 * 0.526868 + 0.529324 ) - 0.0482256 ) ; }
//#line 166
float3 GetIorN ( float3 f0 , float3 edgeTint )
{
float3 sqrtF0 = sqrt ( f0 ) ;
return lerp ( ( 1.0 - f0 ) / ( 1.0 + f0 ) , ( 1.0 + sqrtF0 ) / ( 1.0 - sqrt ( f0 ) ) , edgeTint ) ;
}

float3 getIorK2 ( float3 f0 , float3 n )
{
float3 nf0 = Sq ( n + 1.0 ) * f0 - Sq ( f0 - 1.0 ) ;
return nf0 / ( 1.0 - f0 ) ;
}
//#line 179
float3 CoatRefract ( float3 X , float3 N , float ieta )
{
float XdotN = saturate ( dot ( N , X ) ) ;
return ieta * X + ( sqrt ( 1 + ieta * ieta * ( XdotN * XdotN - 1 ) ) - ieta * XdotN ) * N ;
}
//#line 189
float Lambda_GGX ( float roughness , float3 V )
{
return 0.5 * ( sqrt ( 1.0 + ( Sq ( roughness * V . x ) + Sq ( roughness * V . y ) ) / Sq ( V . z ) ) - 1.0 ) ;
}

float D_GGXNoPI ( float NdotH , float roughness )
{
float a2 = Sq ( roughness ) ;
float s = ( NdotH * a2 - NdotH ) * NdotH + 1.0 ;
//#line 201
return SafeDiv ( a2 , s * s ) ;
}

float D_GGX ( float NdotH , float roughness )
{
return 0.31830988618379067154 * D_GGXNoPI ( NdotH , roughness ) ;
}
//#line 211
float G_MaskingSmithGGX ( float NdotV , float roughness )
{
//#line 220
return 1.0 / ( 0.5 + 0.5 * sqrt ( 1.0 + Sq ( roughness ) * ( 1.0 / Sq ( NdotV ) - 1.0 ) ) ) ;
}
//#line 224
float GetSmithJointGGXPartLambdaV ( float NdotV , float roughness )
{
float a2 = Sq ( roughness ) ;
return sqrt ( ( - NdotV * a2 + NdotV ) * NdotV + a2 ) ;
}
//#line 232
float V_SmithJointGGX ( float NdotL , float NdotV , float roughness , float partLambdaV )
{
float a2 = Sq ( roughness ) ;
//#line 242
float lambdaV = NdotL * partLambdaV ;
float lambdaL = NdotV * sqrt ( ( - NdotL * a2 + NdotL ) * NdotL + a2 ) ;
//#line 246
return 0.5 / max ( lambdaV + lambdaL , 1.175494351e-38 ) ;
}

float V_SmithJointGGX ( float NdotL , float NdotV , float roughness )
{
float partLambdaV = GetSmithJointGGXPartLambdaV ( NdotV , roughness ) ;
return V_SmithJointGGX ( NdotL , NdotV , roughness , partLambdaV ) ;
}
//#line 256
float DV_SmithJointGGX ( float NdotH , float NdotL , float NdotV , float roughness , float partLambdaV )
{
float a2 = Sq ( roughness ) ;
float s = ( NdotH * a2 - NdotH ) * NdotH + 1.0 ;

float lambdaV = NdotL * partLambdaV ;
float lambdaL = NdotV * sqrt ( ( - NdotL * a2 + NdotL ) * NdotL + a2 ) ;

float2 D = float2 ( a2 , s * s ) ;
float2 G = float2 ( 1 , lambdaV + lambdaL ) ;
//#line 270
return 0.31830988618379067154 * 0.5 * ( D . x * G . x ) / max ( D . y * G . y , 1.175494351e-38 ) ;
}

float DV_SmithJointGGX ( float NdotH , float NdotL , float NdotV , float roughness )
{
float partLambdaV = GetSmithJointGGXPartLambdaV ( NdotV , roughness ) ;
return DV_SmithJointGGX ( NdotH , NdotL , NdotV , roughness , partLambdaV ) ;
}
//#line 284
float GetSmithJointGGXPartLambdaVApprox ( float NdotV , float roughness )
{
float a = roughness ;
return NdotV * ( 1 - a ) + a ;
}

float V_SmithJointGGXApprox ( float NdotL , float NdotV , float roughness , float partLambdaV )
{
float a = roughness ;

float lambdaV = NdotL * partLambdaV ;
float lambdaL = NdotV * ( NdotL * ( 1 - a ) + a ) ;

return 0.5 / ( lambdaV + lambdaL ) ;
}

float V_SmithJointGGXApprox ( float NdotL , float NdotV , float roughness )
{
float partLambdaV = GetSmithJointGGXPartLambdaVApprox ( NdotV , roughness ) ;
return V_SmithJointGGXApprox ( NdotL , NdotV , roughness , partLambdaV ) ;
}
//#line 308
float D_GGXAnisoNoPI ( float TdotH , float BdotH , float NdotH , float roughnessT , float roughnessB )
{
float a2 = roughnessT * roughnessB ;
float3 v = float3 ( roughnessB * TdotH , roughnessT * BdotH , a2 * NdotH ) ;
float s = dot ( v , v ) ;
//#line 316
return SafeDiv ( a2 * a2 * a2 , s * s ) ;
}

float D_GGXAniso ( float TdotH , float BdotH , float NdotH , float roughnessT , float roughnessB )
{
return 0.31830988618379067154 * D_GGXAnisoNoPI ( TdotH , BdotH , NdotH , roughnessT , roughnessB ) ;
}

float GetSmithJointGGXAnisoPartLambdaV ( float TdotV , float BdotV , float NdotV , float roughnessT , float roughnessB )
{
return length ( float3 ( roughnessT * TdotV , roughnessB * BdotV , NdotV ) ) ;
}
//#line 331
float V_SmithJointGGXAniso ( float TdotV , float BdotV , float NdotV , float TdotL , float BdotL , float NdotL , float roughnessT , float roughnessB , float partLambdaV )
{
float lambdaV = NdotL * partLambdaV ;
float lambdaL = NdotV * length ( float3 ( roughnessT * TdotL , roughnessB * BdotL , NdotL ) ) ;

return 0.5 / ( lambdaV + lambdaL ) ;
}

float V_SmithJointGGXAniso ( float TdotV , float BdotV , float NdotV , float TdotL , float BdotL , float NdotL , float roughnessT , float roughnessB )
{
float partLambdaV = GetSmithJointGGXAnisoPartLambdaV ( TdotV , BdotV , NdotV , roughnessT , roughnessB ) ;
return V_SmithJointGGXAniso ( TdotV , BdotV , NdotV , TdotL , BdotL , NdotL , roughnessT , roughnessB , partLambdaV ) ;
}
//#line 346
float DV_SmithJointGGXAniso ( float TdotH , float BdotH , float NdotH , float NdotV ,
float TdotL , float BdotL , float NdotL ,
float roughnessT , float roughnessB , float partLambdaV )
{
float a2 = roughnessT * roughnessB ;
float3 v = float3 ( roughnessB * TdotH , roughnessT * BdotH , a2 * NdotH ) ;
float s = dot ( v , v ) ;

float lambdaV = NdotL * partLambdaV ;
float lambdaL = NdotV * length ( float3 ( roughnessT * TdotL , roughnessB * BdotL , NdotL ) ) ;

float2 D = float2 ( a2 * a2 * a2 , s * s ) ;
float2 G = float2 ( 1 , lambdaV + lambdaL ) ;
//#line 363
return ( 0.31830988618379067154 * 0.5 ) * ( D . x * G . x ) / max ( D . y * G . y , 1.175494351e-38 ) ;
}

float DV_SmithJointGGXAniso ( float TdotH , float BdotH , float NdotH ,
float TdotV , float BdotV , float NdotV ,
float TdotL , float BdotL , float NdotL ,
float roughnessT , float roughnessB )
{
float partLambdaV = GetSmithJointGGXAnisoPartLambdaV ( TdotV , BdotV , NdotV , roughnessT , roughnessB ) ;
return DV_SmithJointGGXAniso ( TdotH , BdotH , NdotH , NdotV ,
TdotL , BdotL , NdotL ,
roughnessT , roughnessB , partLambdaV ) ;
}
//#line 380
float GetProjectedRoughness ( float TdotV , float BdotV , float NdotV , float roughnessT , float roughnessB )
{
float2 roughness = float2 ( roughnessT , roughnessB ) ;
float sinTheta2 = max ( ( 1 - Sq ( NdotV ) ) , 1.175494351e-38 ) ;
//#line 390
float2 vProj2 = Sq ( float2 ( TdotV , BdotV ) ) * rcp ( sinTheta2 ) ;

float projRoughness = sqrt ( dot ( vProj2 , roughness * roughness ) ) ;
return projRoughness ;
}
//#line 400
float LambertNoPI ( )
{
return 1.0 ;
}

float Lambert ( )
{
return 0.31830988618379067154 ;
}

float DisneyDiffuseNoPI ( float NdotV , float NdotL , float LdotV , float perceptualRoughness )
{
//#line 414
float fd90 = 0.5 + ( perceptualRoughness + perceptualRoughness * LdotV ) ;

float lightScatter = F_Schlick ( 1.0 , fd90 , NdotL ) ;
float viewScatter = F_Schlick ( 1.0 , fd90 , NdotV ) ;
//#line 425
return rcp ( 1.03571 ) * ( lightScatter * viewScatter ) ;
}
//#line 429
float DisneyDiffuse ( float NdotV , float NdotL , float LdotV , float perceptualRoughness )
{
return 0.31830988618379067154 * DisneyDiffuseNoPI ( NdotV , NdotL , LdotV , perceptualRoughness ) ;
}
//#line 436
float3 DiffuseGGXNoPI ( float3 albedo , float NdotV , float NdotL , float NdotH , float LdotV , float roughness )
{
float facing = 0.5 + 0.5 * LdotV ;
float rough = facing * ( 0.9 - 0.4 * facing ) * ( 0.5 / NdotH + 1 ) ;
float transmitL = F_Transm_Schlick ( 0 , NdotL ) ;
float transmitV = F_Transm_Schlick ( 0 , NdotV ) ;
float smooth = transmitL * transmitV * 1.05 ;
float single = lerp ( smooth , rough , roughness ) ;
float multiple = roughness * ( 0.1159 * 3.14159265358979323846 ) ;

return single + albedo * multiple ;
}

float3 DiffuseGGX ( float3 albedo , float NdotV , float NdotL , float NdotH , float LdotV , float roughness )
{

return 0.31830988618379067154 * DiffuseGGXNoPI ( albedo , NdotV , NdotL , NdotH , LdotV , roughness ) ;
}
//#line 461
float3 EvalSensitivity ( float opd , float shift )
{

float phase = 2.0 * 3.14159265358979323846 * opd * 1e-6 ;
float3 val = float3 ( 5.4856e-13 , 4.4201e-13 , 5.2481e-13 ) ;
float3 pos = float3 ( 1.6810e+06 , 1.7953e+06 , 2.2084e+06 ) ;
float3 var = float3 ( 4.3278e+09 , 9.3046e+09 , 6.6121e+09 ) ;
float3 xyz = val * sqrt ( 2.0 * 3.14159265358979323846 * var ) * cos ( pos * phase + shift ) * exp ( - var * phase * phase ) ;
xyz . x += 9.7470e-14 * sqrt ( 2.0 * 3.14159265358979323846 * 4.5282e+09 ) * cos ( 2.2399e+06 * phase + shift ) * exp ( - 4.5282e+09 * phase * phase ) ;
xyz /= 1.0685e-7 ;
//#line 474
float3 srgb = mul ( XYZ_2_REC709_MAT , xyz ) ;
return srgb ;
}
//#line 479
float3 EvalIridescence ( float eta_1 , float cosTheta1 , float iridescenceThickness , float3 baseLayerFresnel0 , float iorOverBaseLayer = 0.0 )
{
float3 I ;
//#line 484
float Dinc = 3.0 * iridescenceThickness ;
//#line 493
float eta_2 = lerp ( 2.0 , 1.0 , iridescenceThickness ) ;
//#line 498
float sinTheta2Sq = Sq ( eta_1 / eta_2 ) * ( 1.0 - Sq ( cosTheta1 ) ) ;
//#line 504
float cosTheta2Sq = ( 1.0 - sinTheta2Sq ) ;
//#line 509
if ( cosTheta2Sq < 0.0 )
I = float3 ( 1.0 , 1.0 , 1.0 ) ;
else
{

float cosTheta2 = sqrt ( cosTheta2Sq ) ;
//#line 517
float R0 = IorToFresnel0 ( eta_2 , eta_1 ) ;
float R12 = F_Schlick ( R0 , cosTheta1 ) ;
float R21 = R12 ;
float T121 = 1.0 - R12 ;
float phi12 = 0.0 ;
float phi21 = 3.14159265358979323846 - phi12 ;
//#line 528
if ( iorOverBaseLayer > 0.0 )
{

float3 baseIor = iorOverBaseLayer * Fresnel0ToIor ( baseLayerFresnel0 + 0.0001 ) ;
baseLayerFresnel0 = IorToFresnel0 ( baseIor , eta_2 ) ;
}

float3 R23 = F_Schlick ( baseLayerFresnel0 , cosTheta2 ) ;
float phi23 = 0.0 ;
//#line 539
float OPD = Dinc * cosTheta2 ;
float phi = phi21 + phi23 ;
//#line 543
float3 R123 = clamp ( R12 * R23 , 1e-5 , 0.9999 ) ;
float3 r123 = sqrt ( R123 ) ;
float3 Rs = Sq ( T121 ) * R23 / ( float3 ( 1.0 , 1.0 , 1.0 ) - R123 ) ;
//#line 548
float3 C0 = R12 + Rs ;
I = C0 ;
//#line 552
float3 Cm = Rs - T121 ;
for ( int m = 1 ; m <= 2 ; ++ m )
{
Cm *= r123 ;
float3 Sm = 2.0 * EvalSensitivity ( m * OPD , m * phi ) ;

I += Cm * Sm ;
}
//#line 562
I = max ( I , float3 ( 0.0 , 0.0 , 0.0 ) ) ;
}

return I ;
}
//#line 573
float D_CharlieNoPI ( float NdotH , float roughness )
{
float invR = rcp ( roughness ) ;
float cos2h = NdotH * NdotH ;
float sin2h = 1.0 - cos2h ;

return ( 2.0 + invR ) * PositivePow ( sin2h , invR * 0.5 ) / 2.0 ;
}

float D_Charlie ( float NdotH , float roughness )
{
return 0.31830988618379067154 * D_CharlieNoPI ( NdotH , roughness ) ;
}

float CharlieL ( float x , float r )
{
r = saturate ( r ) ;
r = 1.0 - ( 1.0 - r ) * ( 1.0 - r ) ;

float a = lerp ( 25.3245 , 21.5473 , r ) ;
float b = lerp ( 3.32435 , 3.82987 , r ) ;
float c = lerp ( 0.16801 , 0.19823 , r ) ;
float d = lerp ( - 1.27393 , - 1.97760 , r ) ;
float e = lerp ( - 4.85967 , - 4.32054 , r ) ;

return a / ( 1. + b * PositivePow ( x , c ) ) + d * x + e ;
}
//#line 602
float V_Charlie ( float NdotL , float NdotV , float roughness )
{
float lambdaV = NdotV < 0.5 ? exp ( CharlieL ( NdotV , roughness ) ) : exp ( 2.0 * CharlieL ( 0.5 , roughness ) - CharlieL ( 1.0 - NdotV , roughness ) ) ;
float lambdaL = NdotL < 0.5 ? exp ( CharlieL ( NdotL , roughness ) ) : exp ( 2.0 * CharlieL ( 0.5 , roughness ) - CharlieL ( 1.0 - NdotL , roughness ) ) ;

return 1.0 / ( ( 1.0 + lambdaV + lambdaL ) * ( 4.0 * NdotV * NdotL ) ) ;
}
//#line 611
float V_Ashikhmin ( float NdotL , float NdotV )
{

return 1.0 / ( 4.0 * ( NdotL + NdotV - NdotL * NdotV ) ) ;
}
//#line 618
float FabricLambertNoPI ( float roughness )
{
return lerp ( 1.0 , 0.5 , roughness ) ;
}

float FabricLambert ( float roughness )
{
return 0.31830988618379067154 * FabricLambertNoPI ( roughness ) ;
}

float G_CookTorrance ( float NdotH , float NdotV , float NdotL , float HdotV )
{
return min ( 1.0 , 2.0 * NdotH * min ( NdotV , NdotL ) / HdotV ) ;
}
//#line 638
float3 ShiftTangent ( float3 T , float3 N , float shift )
{
return normalize ( T + N * shift ) ;
}
//#line 644
float3 D_KajiyaKay ( float3 T , float3 H , float specularExponent )
{
float TdotH = dot ( T , H ) ;
float sinTHSq = saturate ( 1.0 - TdotH * TdotH ) ;

float dirAttn = saturate ( TdotH + 1.0 ) ;
//#line 655
float n = specularExponent ;
float norm = ( n + 2 ) * rcp ( 2 * 3.14159265358979323846 ) ;

return dirAttn * norm * PositivePow ( sinTHSq , 0.5 * n ) ;
}
//#line 11 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
struct BRDFData
{
half3 albedo ;
half3 diffuse ;
half3 specular ;
half reflectivity ;
half perceptualRoughness ;
half roughness ;
half roughness2 ;
half grazingTerm ;
//#line 24
half normalizationTerm ;
half roughness2MinusOne ;
} ;

half ReflectivitySpecular ( half3 specular )
{
return Max3 ( specular . r , specular . g , specular . b ) ;
}

half OneMinusReflectivityMetallic ( half metallic )
{
//#line 40
half oneMinusDielectricSpec = half4 ( 0.04 , 0.04 , 0.04 , 1.0 - 0.04 ) . a ;
return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec ;
}

half MetallicFromReflectivity ( half reflectivity )
{
half oneMinusDielectricSpec = half4 ( 0.04 , 0.04 , 0.04 , 1.0 - 0.04 ) . a ;
return ( reflectivity - half4 ( 0.04 , 0.04 , 0.04 , 1.0 - 0.04 ) . r ) / oneMinusDielectricSpec ;
}

inline void InitializeBRDFDataDirect ( half3 albedo , half3 diffuse , half3 specular , half reflectivity , half oneMinusReflectivity , half smoothness , inout half alpha , out BRDFData outBRDFData )
{
outBRDFData = ( BRDFData ) 0 ;
outBRDFData . albedo = albedo ;
outBRDFData . diffuse = diffuse ;
outBRDFData . specular = specular ;
outBRDFData . reflectivity = reflectivity ;

outBRDFData . perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness ( smoothness ) ;
outBRDFData . roughness = max ( PerceptualRoughnessToRoughness ( outBRDFData . perceptualRoughness ) , 0.0078125 ) ;
outBRDFData . roughness2 = max ( outBRDFData . roughness * outBRDFData . roughness , 6.103515625e-5 ) ;
outBRDFData . grazingTerm = saturate ( smoothness + reflectivity ) ;
outBRDFData . normalizationTerm = outBRDFData . roughness * half ( 4.0 ) + half ( 2.0 ) ;
outBRDFData . roughness2MinusOne = outBRDFData . roughness2 - half ( 1.0 ) ;
//#line 73
}
//#line 76
inline void InitializeBRDFDataDirect ( half3 diffuse , half3 specular , half reflectivity , half oneMinusReflectivity , half smoothness , inout half alpha , out BRDFData outBRDFData )
{
InitializeBRDFDataDirect ( half3 ( 0.0 , 0.0 , 0.0 ) , diffuse , specular , reflectivity , oneMinusReflectivity , smoothness , alpha , outBRDFData ) ;
}
//#line 82
inline void InitializeBRDFData ( half3 albedo , half metallic , half3 specular , half smoothness , inout half alpha , out BRDFData outBRDFData )
{
//#line 90
half oneMinusReflectivity = OneMinusReflectivityMetallic ( metallic ) ;
half reflectivity = half ( 1.0 ) - oneMinusReflectivity ;
half3 brdfDiffuse = albedo * oneMinusReflectivity ;
half3 brdfSpecular = lerp ( half4 ( 0.04 , 0.04 , 0.04 , 1.0 - 0.04 ) . rgb , albedo , metallic ) ;
//#line 96
InitializeBRDFDataDirect ( albedo , brdfDiffuse , brdfSpecular , reflectivity , oneMinusReflectivity , smoothness , alpha , outBRDFData ) ;
}

inline void InitializeBRDFData ( inout SurfaceData surfaceData , out BRDFData brdfData )
{
InitializeBRDFData ( surfaceData . albedo , surfaceData . metallic , surfaceData . specular , surfaceData . smoothness , surfaceData . alpha , brdfData ) ;
}

half3 ConvertF0ForClearCoat15 ( half3 f0 )
{
return ConvertF0ForAirInterfaceToF0ForClearCoat15Fast ( f0 ) ;
}

inline void InitializeBRDFDataClearCoat ( half clearCoatMask , half clearCoatSmoothness , inout BRDFData baseBRDFData , out BRDFData outBRDFData )
{
outBRDFData = ( BRDFData ) 0 ;
outBRDFData . albedo = half ( 1.0 ) ;
//#line 115
outBRDFData . diffuse = half4 ( 0.04 , 0.04 , 0.04 , 1.0 - 0.04 ) . aaa ;
outBRDFData . specular = half4 ( 0.04 , 0.04 , 0.04 , 1.0 - 0.04 ) . rgb ;
outBRDFData . reflectivity = half4 ( 0.04 , 0.04 , 0.04 , 1.0 - 0.04 ) . r ;

outBRDFData . perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness ( clearCoatSmoothness ) ;
outBRDFData . roughness = max ( PerceptualRoughnessToRoughness ( outBRDFData . perceptualRoughness ) , 0.0078125 ) ;
outBRDFData . roughness2 = max ( outBRDFData . roughness * outBRDFData . roughness , 6.103515625e-5 ) ;
outBRDFData . normalizationTerm = outBRDFData . roughness * half ( 4.0 ) + half ( 2.0 ) ;
outBRDFData . roughness2MinusOne = outBRDFData . roughness2 - half ( 1.0 ) ;
outBRDFData . grazingTerm = saturate ( clearCoatSmoothness + half4 ( 0.04 , 0.04 , 0.04 , 1.0 - 0.04 ) . x ) ;
//#line 127
half ieta = lerp ( 1.0h , ( 1.0 / 1.5 ) , clearCoatMask ) ;
half coatRoughnessScale = Sq ( ieta ) ;
half sigma = RoughnessToVariance ( PerceptualRoughnessToRoughness ( baseBRDFData . perceptualRoughness ) ) ;

baseBRDFData . perceptualRoughness = RoughnessToPerceptualRoughness ( VarianceToRoughness ( sigma * coatRoughnessScale ) ) ;
//#line 134
baseBRDFData . roughness = max ( PerceptualRoughnessToRoughness ( baseBRDFData . perceptualRoughness ) , 0.0078125 ) ;
baseBRDFData . roughness2 = max ( baseBRDFData . roughness * baseBRDFData . roughness , 6.103515625e-5 ) ;
baseBRDFData . normalizationTerm = baseBRDFData . roughness * 4.0h + 2.0h ;
baseBRDFData . roughness2MinusOne = baseBRDFData . roughness2 - 1.0h ;
//#line 140
baseBRDFData . specular = lerp ( baseBRDFData . specular , ConvertF0ForClearCoat15 ( baseBRDFData . specular ) , clearCoatMask ) ;

}

BRDFData CreateClearCoatBRDFData ( SurfaceData surfaceData , inout BRDFData brdfData )
{
BRDFData brdfDataClearCoat = ( BRDFData ) 0 ;
//#line 153
return brdfDataClearCoat ;
}
//#line 157
half3 EnvironmentBRDFSpecular ( BRDFData brdfData , half fresnelTerm )
{
float surfaceReduction = 1.0 / ( brdfData . roughness2 + 1.0 ) ;
return half3 ( surfaceReduction * lerp ( brdfData . specular , brdfData . grazingTerm , fresnelTerm ) ) ;
}

half3 EnvironmentBRDF ( BRDFData brdfData , half3 indirectDiffuse , half3 indirectSpecular , half fresnelTerm )
{
half3 c = indirectDiffuse * brdfData . diffuse ;
c += indirectSpecular * EnvironmentBRDFSpecular ( brdfData , fresnelTerm ) ;
return c ;
}
//#line 171
half3 EnvironmentBRDFClearCoat ( BRDFData brdfData , half clearCoatMask , half3 indirectSpecular , half fresnelTerm )
{
float surfaceReduction = 1.0 / ( brdfData . roughness2 + 1.0 ) ;
return indirectSpecular * EnvironmentBRDFSpecular ( brdfData , fresnelTerm ) * clearCoatMask ;
}
//#line 179
half DirectBRDFSpecular ( BRDFData brdfData , half3 normalWS , half3 lightDirectionWS , half3 viewDirectionWS )
{
float3 lightDirectionWSFloat3 = float3 ( lightDirectionWS ) ;
float3 halfDir = SafeNormalize ( lightDirectionWSFloat3 + float3 ( viewDirectionWS ) ) ;

float NoH = saturate ( dot ( float3 ( normalWS ) , halfDir ) ) ;
half LoH = half ( saturate ( dot ( lightDirectionWSFloat3 , halfDir ) ) ) ;
//#line 197
float d = NoH * NoH * brdfData . roughness2MinusOne + 1.00001f ;

half LoH2 = LoH * LoH ;
half specularTerm = brdfData . roughness2 / ( ( d * d ) * max ( 0.1h , LoH2 ) * brdfData . normalizationTerm ) ;
//#line 214
return specularTerm ;
}
//#line 223
half3 DirectBDRF ( BRDFData brdfData , half3 normalWS , half3 lightDirectionWS , half3 viewDirectionWS , bool specularHighlightsOff )
{
//#line 227
[ branch ] if ( ! specularHighlightsOff )
{
half specularTerm = DirectBRDFSpecular ( brdfData , normalWS , lightDirectionWS , viewDirectionWS ) ;
half3 color = brdfData . diffuse + specularTerm * brdfData . specular ;
return color ;
}
else
return brdfData . diffuse ;
}
//#line 243
half3 DirectBRDF ( BRDFData brdfData , half3 normalWS , half3 lightDirectionWS , half3 viewDirectionWS )
{

return brdfData . diffuse + DirectBRDFSpecular ( brdfData , normalWS , lightDirectionWS , viewDirectionWS ) * brdfData . specular ;
//#line 250
}
//#line 15 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/SphericalHarmonics.hlsl"
static const float kSHBasisCoef [ ] = { 0.28209479177387814347f , - 0.48860251190291992159f , 0.48860251190291992159f , - 0.48860251190291992159f , 1.09254843059207907054f , - 1.09254843059207907054f , 0.31539156525252000603f , - 1.09254843059207907054f , 0.54627421529603953527f } ;
//#line 23
static const float kClampedCosineCoefs [ ] = { ( 1.0f ) , ( 2.0f / 3.0f ) , ( 2.0f / 3.0f ) , ( 2.0f / 3.0f ) , ( 1.0f / 4.0f ) , ( 1.0f / 4.0f ) , ( 1.0f / 4.0f ) , ( 1.0f / 4.0f ) , ( 1.0f / 4.0f ) } ;
//#line 26
float3 SHEvalLinearL0L1 ( float3 N , float4 shAr , float4 shAg , float4 shAb )
{
float4 vA = float4 ( N , 1.0 ) ;

float3 x1 ;

x1 . r = dot ( shAr , vA ) ;
x1 . g = dot ( shAg , vA ) ;
x1 . b = dot ( shAb , vA ) ;

return x1 ;
}

float3 SHEvalLinearL1 ( float3 N , float3 shAr , float3 shAg , float3 shAb )
{
float3 x1 ;
x1 . r = dot ( shAr , N ) ;
x1 . g = dot ( shAg , N ) ;
x1 . b = dot ( shAb , N ) ;

return x1 ;
}

float3 SHEvalLinearL2 ( float3 N , float4 shBr , float4 shBg , float4 shBb , float4 shC )
{
float3 x2 ;

float4 vB = N . xyzz * N . yzzx ;
x2 . r = dot ( shBr , vB ) ;
x2 . g = dot ( shBg , vB ) ;
x2 . b = dot ( shBb , vB ) ;
//#line 59
float vC = N . x * N . x - N . y * N . y ;
float3 x3 = shC . rgb * vC ;

return x2 + x3 ;
}
//#line 106
float3 SampleSH9 ( float4 SHCoefficients [ 7 ] , float3 N )
{
float4 shAr = SHCoefficients [ 0 ] ;
float4 shAg = SHCoefficients [ 1 ] ;
float4 shAb = SHCoefficients [ 2 ] ;
float4 shBr = SHCoefficients [ 3 ] ;
float4 shBg = SHCoefficients [ 4 ] ;
float4 shBb = SHCoefficients [ 5 ] ;
float4 shCr = SHCoefficients [ 6 ] ;
//#line 117
float3 res = SHEvalLinearL0L1 ( N , shAr , shAg , shAb ) ;
//#line 120
res += SHEvalLinearL2 ( N , shBr , shBg , shBb , shCr ) ;
//#line 126
return res ;
}

float3 SampleSH9 ( StructuredBuffer < float4 > data , float3 N )
{
float4 SHCoefficients [ 7 ] ;
SHCoefficients [ 0 ] = data [ 0 ] ;
SHCoefficients [ 1 ] = data [ 1 ] ;
SHCoefficients [ 2 ] = data [ 2 ] ;
SHCoefficients [ 3 ] = data [ 3 ] ;
SHCoefficients [ 4 ] = data [ 4 ] ;
SHCoefficients [ 5 ] = data [ 5 ] ;
SHCoefficients [ 6 ] = data [ 6 ] ;

return SampleSH9 ( SHCoefficients , N ) ;
}

float3 SampleSH4_L1 ( float4 SHCoefficients [ 3 ] , float3 N )
{
float4 shAr = SHCoefficients [ 0 ] ;
float4 shAg = SHCoefficients [ 1 ] ;
float4 shAb = SHCoefficients [ 2 ] ;
//#line 150
float3 res = SHEvalLinearL1 ( N , ( float3 ) shAr , ( float3 ) shAg , ( float3 ) shAb ) ;
//#line 156
return res ;
}

void GetCornetteShanksPhaseFunction ( out float3 zh , float anisotropy )
{
float g = anisotropy ;

zh . x = 0.282095f ;
zh . y = 0.293162f * g * ( 4.0f + ( g * g ) ) / ( 2.0f + ( g * g ) ) ;
zh . z = ( 0.126157f + 1.44179f * ( g * g ) + 0.324403f * ( g * g ) * ( g * g ) ) / ( 2.0f + ( g * g ) ) ;
}

void ConvolveZonal ( inout float sh [ 27 ] , float3 zh )
{
for ( int l = 0 ; l <= 2 ; l ++ )
{
float n = sqrt ( ( 4.0f * 3.14159265358979323846 ) / ( 2 * l + 1 ) ) ;
float k = zh [ l ] ;
float p = n * k ;

for ( int m = - l ; m <= l ; m ++ )
{
int i = l * ( l + 1 ) + m ;

for ( int c = 0 ; c < 3 ; c ++ )
{
sh [ c * 9 + i ] = sh [ c * 9 + i ] * p ;
}
}
}
}
//#line 192
void PackSH ( RWStructuredBuffer < float4 > buffer , float sh [ 27 ] )
{
int c = 0 ;
for ( c = 0 ; c < 3 ; c ++ )

{
buffer [ c ] = float4 ( sh [ c * 9 + 3 ] , sh [ c * 9 + 1 ] , sh [ c * 9 + 2 ] , sh [ c * 9 + 0 ] - sh [ c * 9 + 6 ] ) ;
}
//#line 202
for ( c = 0 ; c < 3 ; c ++ )
{
buffer [ 3 + c ] = float4 ( sh [ c * 9 + 4 ] , sh [ c * 9 + 5 ] , sh [ c * 9 + 6 ] * 3.0f , sh [ c * 9 + 7 ] ) ;
}
//#line 208
buffer [ 6 ] = float4 ( sh [ 0 * 9 + 8 ] , sh [ 1 * 9 + 8 ] , sh [ 2 * 9 + 8 ] , 1.0f ) ;
}
//#line 41 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
void SampleProbeVolumeSH4 ( Texture3D SHVolumeTexture , SamplerState SHVolumeSampler , float3 positionWS , float3 normalWS , float3 backNormalWS , float4x4 WorldToTexture ,
float transformToLocal , float texelSizeX , float3 probeVolumeMin , float3 probeVolumeSizeInv ,
inout float3 bakeDiffuseLighting , inout float3 backBakeDiffuseLighting )
{
float3 position = ( transformToLocal == 1.0 ) ? mul ( WorldToTexture , float4 ( positionWS , 1.0 ) ) . xyz : positionWS ;
float3 texCoord = ( position - probeVolumeMin ) * probeVolumeSizeInv . xyz ;
//#line 52
texCoord . x = clamp ( texCoord . x * 0.25 , 0.5 * texelSizeX , 0.25 - 0.5 * texelSizeX ) ;

float4 shAr = SHVolumeTexture . SampleLevel ( SHVolumeSampler , texCoord , 0 ) ;
texCoord . x += 0.25 ;
float4 shAg = SHVolumeTexture . SampleLevel ( SHVolumeSampler , texCoord , 0 ) ;
texCoord . x += 0.25 ;
float4 shAb = SHVolumeTexture . SampleLevel ( SHVolumeSampler , texCoord , 0 ) ;

bakeDiffuseLighting += SHEvalLinearL0L1 ( normalWS , shAr , shAg , shAb ) ;
backBakeDiffuseLighting += SHEvalLinearL0L1 ( backNormalWS , shAr , shAg , shAb ) ;
}
//#line 65
float3 SampleProbeVolumeSH4 ( Texture3D SHVolumeTexture , SamplerState SHVolumeSampler , float3 positionWS , float3 normalWS , float4x4 WorldToTexture ,
float transformToLocal , float texelSizeX , float3 probeVolumeMin , float3 probeVolumeSizeInv )
{
float3 backNormalWSUnused = 0.0 ;
float3 bakeDiffuseLighting = 0.0 ;
float3 backBakeDiffuseLightingUnused = 0.0 ;
SampleProbeVolumeSH4 ( SHVolumeTexture , SHVolumeSampler , positionWS , normalWS , backNormalWSUnused , WorldToTexture ,
transformToLocal , texelSizeX , probeVolumeMin , probeVolumeSizeInv ,
bakeDiffuseLighting , backBakeDiffuseLightingUnused ) ;
return bakeDiffuseLighting ;
}
//#line 81
void SampleProbeVolumeSH9 ( Texture3D SHVolumeTexture , SamplerState SHVolumeSampler , float3 positionWS , float3 normalWS , float3 backNormalWS , float4x4 WorldToTexture ,
float transformToLocal , float texelSizeX , float3 probeVolumeMin , float3 probeVolumeSizeInv ,
inout float3 bakeDiffuseLighting , inout float3 backBakeDiffuseLighting )
{
float3 position = ( transformToLocal == 1.0f ) ? mul ( WorldToTexture , float4 ( positionWS , 1.0 ) ) . xyz : positionWS ;
float3 texCoord = ( position - probeVolumeMin ) * probeVolumeSizeInv ;

const uint shCoeffCount = 7 ;
const float invShCoeffCount = 1.0f / float ( shCoeffCount ) ;
//#line 92
texCoord . x = texCoord . x / shCoeffCount ;
//#line 95
float texCoordX = clamp ( texCoord . x , 0.5f * texelSizeX , invShCoeffCount - 0.5f * texelSizeX ) ;

float4 SHCoefficients [ 7 ] ;

for ( uint i = 0 ; i < shCoeffCount ; i ++ )
{
texCoord . x = texCoordX + i * invShCoeffCount ;
SHCoefficients [ i ] = SHVolumeTexture . SampleLevel ( SHVolumeSampler , texCoord , 0 ) ;
}

bakeDiffuseLighting += SampleSH9 ( SHCoefficients , normalize ( normalWS ) ) ;
backBakeDiffuseLighting += SampleSH9 ( SHCoefficients , normalize ( backNormalWS ) ) ;
}
//#line 110
float3 SampleProbeVolumeSH9 ( Texture3D SHVolumeTexture , SamplerState SHVolumeSampler , float3 positionWS , float3 normalWS , float4x4 WorldToTexture ,
float transformToLocal , float texelSizeX , float3 probeVolumeMin , float3 probeVolumeSizeInv )
{
float3 backNormalWSUnused = 0.0 ;
float3 bakeDiffuseLighting = 0.0 ;
float3 backBakeDiffuseLightingUnused = 0.0 ;
SampleProbeVolumeSH9 ( SHVolumeTexture , SHVolumeSampler , positionWS , normalWS , backNormalWSUnused , WorldToTexture ,
transformToLocal , texelSizeX , probeVolumeMin , probeVolumeSizeInv ,
bakeDiffuseLighting , backBakeDiffuseLightingUnused ) ;
return bakeDiffuseLighting ;
}

float4 SampleProbeOcclusion ( Texture3D SHVolumeTexture , SamplerState SHVolumeSampler , float3 positionWS , float4x4 WorldToTexture ,
float transformToLocal , float texelSizeX , float3 probeVolumeMin , float3 probeVolumeSizeInv )
{
float3 position = ( transformToLocal == 1.0 ) ? mul ( WorldToTexture , float4 ( positionWS , 1.0 ) ) . xyz : positionWS ;
float3 texCoord = ( position - probeVolumeMin ) * probeVolumeSizeInv . xyz ;
//#line 131
texCoord . x = max ( texCoord . x * 0.25 + 0.75 , 0.75 + 0.5 * texelSizeX ) ;

return SHVolumeTexture . Sample ( SHVolumeSampler , texCoord ) ;
}
//#line 150
float4 PackEmissiveRGBM ( float3 rgb )
{
float kOneOverRGBMMaxRange = 1.0 / 97.0 ;
const float kMinMultiplier = 2.0 * 1e-2 ;

float4 rgbm = float4 ( rgb * kOneOverRGBMMaxRange , 1.0 ) ;
rgbm . a = max ( max ( rgbm . r , rgbm . g ) , max ( rgbm . b , kMinMultiplier ) ) ;
rgbm . a = ceil ( rgbm . a * 255.0 ) / 255.0 ;
//#line 160
rgbm . a = max ( rgbm . a , kMinMultiplier ) ;

rgbm . rgb /= rgbm . a ;
return rgbm ;
}

float3 UnpackLightmapRGBM ( float4 rgbmInput , float4 decodeInstructions )
{
//#line 171
return rgbmInput . rgb * ( PositivePow ( rgbmInput . a , decodeInstructions . y ) * decodeInstructions . x ) ;

}

float3 UnpackLightmapDoubleLDR ( float4 encodedColor , float4 decodeInstructions )
{
return encodedColor . rgb * decodeInstructions . x ;
}
//#line 181
float3 DecodeLightmap ( float4 encodedIlluminance , float4 decodeInstructions )
{
//#line 188
return encodedIlluminance . rgb ;

}
//#line 193
float3 DecodeHDREnvironment ( float4 encodedIrradiance , float4 decodeInstructions )
{

float alpha = max ( decodeInstructions . w * ( encodedIrradiance . a - 1.0 ) + 1.0 , 0.0 ) ;
//#line 199
return ( decodeInstructions . x * PositivePow ( alpha , decodeInstructions . y ) ) * encodedIrradiance . rgb ;
}
//#line 240
float3 SampleSingleLightmap ( Texture2D lightmapTex , SamplerState lightmapSampler , float2 uv , float4 transform , bool isStaticLightmap )
{
float4 decodeInstructions = float4 ( float ( 1.0 ) , float ( 1.0 ) , 0.0 , 0.0 ) ;
//#line 245
uv = uv * transform . xy + transform . zw ;
float4 encodedIlluminance = lightmapTex . SampleBias ( lightmapSampler , uv , _GlobalMipBias . x ) . rgba ;

float3 illuminance = isStaticLightmap ? DecodeLightmap ( encodedIlluminance , decodeInstructions ) : encodedIlluminance . rgb ;

return illuminance ;
}
//#line 254
float3 SampleSingleLightmap ( Texture2D lightmapTex , SamplerState lightmapSampler , float2 uv , float4 transform , bool isStaticLightmap , float4 ignore )
{
return SampleSingleLightmap ( lightmapTex , lightmapSampler , uv , transform , isStaticLightmap ) ;
}

void SampleDirectionalLightmap ( Texture2D lightmapTex , SamplerState lightmapSampler , Texture2D lightmapDirTex , SamplerState lightmapDirSampler , float2 uv , float4 transform ,
float3 normalWS , float3 backNormalWS , bool isStaticLightmap , inout float3 bakeDiffuseLighting , inout float3 backBakeDiffuseLighting )
{
//#line 269
float3 illuminance = SampleSingleLightmap ( lightmapTex , lightmapSampler , uv , transform , isStaticLightmap ) ;
//#line 272
uv = uv * transform . xy + transform . zw ;

float4 direction = lightmapDirTex . SampleBias ( lightmapDirSampler , uv , _GlobalMipBias . x ) ;

float halfLambert = dot ( normalWS , direction . xyz - 0.5 ) + 0.5 ;
bakeDiffuseLighting += illuminance * halfLambert / max ( 1e-4 , direction . w ) ;

float backHalfLambert = dot ( backNormalWS , direction . xyz - 0.5 ) + 0.5 ;
backBakeDiffuseLighting += illuminance * backHalfLambert / max ( 1e-4 , direction . w ) ;
}
//#line 284
void SampleDirectionalLightmap ( Texture2D lightmapTex , SamplerState lightmapSampler , Texture2D lightmapDirTex , SamplerState lightmapDirSampler , float2 uv , float4 transform ,
float3 normalWS , float3 backNormalWS , bool isStaticLightmap , float4 ignore , inout float3 bakeDiffuseLighting , inout float3 backBakeDiffuseLighting )
{
SampleDirectionalLightmap ( lightmapTex , lightmapSampler , lightmapDirTex , lightmapDirSampler , uv ,
transform , normalWS , backNormalWS , isStaticLightmap , bakeDiffuseLighting , backBakeDiffuseLighting ) ;
}
//#line 292
float3 SampleDirectionalLightmap ( Texture2D lightmapTex , SamplerState lightmapSampler , Texture2D lightmapDirTex , SamplerState lightmapDirSampler , float2 uv , float4 transform , float3 normalWS , bool isStaticLightmap )
{
float3 backNormalWSUnused = 0.0 ;
float3 bakeDiffuseLighting = 0.0 ;
float3 backBakeDiffuseLightingUnused = 0.0 ;
SampleDirectionalLightmap ( lightmapTex , lightmapSampler , lightmapDirTex , lightmapDirSampler , uv , transform ,
normalWS , backNormalWSUnused , isStaticLightmap , bakeDiffuseLighting , backBakeDiffuseLightingUnused ) ;

return bakeDiffuseLighting ;
}
//#line 304
float3 SampleDirectionalLightmap ( Texture2D lightmapTex , SamplerState lightmapSampler , Texture2D lightmapDirTex , SamplerState lightmapDirSampler , float2 uv , float4 transform , float3 normalWS ,
bool isStaticLightmap , float4 ignore )
{
return SampleDirectionalLightmap ( lightmapTex , lightmapSampler , lightmapDirTex , lightmapDirSampler , uv , transform , normalWS , isStaticLightmap ) ;
}
//#line 19 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/CommonLighting.hlsl"
float3 MapCubeToSphere ( float3 v )
{
float3 v2 = v * v ;
float2 vr3 = v2 . xy * rcp ( 3.0 ) ;
return v * sqrt ( ( float3 ) 1.0 - 0.5 * v2 . yzx - 0.5 * v2 . zxy + vr3 . yxx * v2 . zzy ) ;
}
//#line 27
float ComputeCubeToSphereMapSqMagnitude ( float3 v )
{
float3 v2 = v * v ;
//#line 32
return dot ( v , v ) - v2 . x * v2 . y - v2 . y * v2 . z - v2 . z * v2 . x + v2 . x * v2 . y * v2 . z ;
}
//#line 39
float ComputeCubemapTexelSolidAngle ( float3 L , float texelArea )
{

float d = Max3 ( abs ( L . x ) , abs ( L . y ) , abs ( L . z ) ) ;
//#line 45
float invDist = d ;
//#line 49
return texelArea * invDist * invDist * invDist ;
}
//#line 56
float ComputeCubemapTexelSolidAngle ( float2 uv )
{
float u = uv . x , v = uv . y ;
return pow ( 1 + u * u + v * v , - 1.5 ) ;
}

float ConvertEvToLuminance ( float ev )
{
return exp2 ( ev - 3.0 ) ;
}

float ConvertLuminanceToEv ( float luminance )
{
float k = 12.5f ;

return log2 ( ( luminance * 100.0 ) / k ) ;
}
//#line 82
float DistanceWindowing ( float distSquare , float rangeAttenuationScale , float rangeAttenuationBias )
{
//#line 90
return saturate ( rangeAttenuationBias - Sq ( distSquare * rangeAttenuationScale ) ) ;
}

float SmoothDistanceWindowing ( float distSquare , float rangeAttenuationScale , float rangeAttenuationBias )
{
float factor = DistanceWindowing ( distSquare , rangeAttenuationScale , rangeAttenuationBias ) ;
return Sq ( factor ) ;
}
//#line 102
float SmoothWindowedDistanceAttenuation ( float distSquare , float distRcp , float rangeAttenuationScale , float rangeAttenuationBias )
{
float attenuation = min ( distRcp , 1.0 / 0.01 ) ;
attenuation *= DistanceWindowing ( distSquare , rangeAttenuationScale , rangeAttenuationBias ) ;
//#line 108
return Sq ( attenuation ) ;
}
//#line 112
float AngleAttenuation ( float cosFwd , float lightAngleScale , float lightAngleOffset )
{
return saturate ( cosFwd * lightAngleScale + lightAngleOffset ) ;
}

float SmoothAngleAttenuation ( float cosFwd , float lightAngleScale , float lightAngleOffset )
{
float attenuation = AngleAttenuation ( cosFwd , lightAngleScale , lightAngleOffset ) ;
return Sq ( attenuation ) ;
}
//#line 125
float PunctualLightAttenuation ( float4 distances , float rangeAttenuationScale , float rangeAttenuationBias ,
float lightAngleScale , float lightAngleOffset )
{
float distSq = distances . y ;
float distRcp = distances . z ;
float distProj = distances . w ;
float cosFwd = distProj * distRcp ;

float attenuation = min ( distRcp , 1.0 / 0.01 ) ;
attenuation *= DistanceWindowing ( distSq , rangeAttenuationScale , rangeAttenuationBias ) ;
attenuation *= AngleAttenuation ( cosFwd , lightAngleScale , lightAngleOffset ) ;
//#line 138
return Sq ( attenuation ) ;
}
//#line 145
float CapsuleWindowing ( float3 center , float3 xAxis , float halfLength ,
float rangeAttenuationScale , float rangeAttenuationBias )
{
//#line 155
float x = dot ( center , xAxis ) ;
float dx = max ( 0 , abs ( x ) - halfLength ) ;
float r2 = dot ( center , center ) ;
float z2 = max ( 0 , r2 - x * x ) ;
float d2 = z2 + dx * dx ;

return SmoothDistanceWindowing ( d2 , rangeAttenuationScale , rangeAttenuationBias ) ;
}
//#line 169
float PillowWindowing ( float3 center , float3 xAxis , float3 yAxis , float halfLength , float halfHeight ,
float rangeAttenuationScale , float rangeAttenuationBias )
{
//#line 179
float x = dot ( center , xAxis ) ;
float dx = max ( 0 , abs ( x ) - halfLength ) ;
float y = dot ( center , yAxis ) ;
float dy = max ( 0 , abs ( y ) - halfHeight ) ;
float r2 = dot ( center , center ) ;
float z2 = max ( 0 , r2 - x * x - y * y ) ;
float d2 = z2 + dx * dx + dy * dy ;

return SmoothDistanceWindowing ( d2 , rangeAttenuationScale , rangeAttenuationBias ) ;
}
//#line 195
float EllipsoidalDistanceAttenuation ( float3 unL , float3 axis , float invAspectRatio ,
float rangeAttenuationScale , float rangeAttenuationBias )
{

float projL = dot ( unL , axis ) ;
//#line 203
float diff = projL - projL * invAspectRatio ;
unL -= diff * axis ;

float sqDist = dot ( unL , unL ) ;
return SmoothDistanceWindowing ( sqDist , rangeAttenuationScale , rangeAttenuationBias ) ;
}
//#line 213
float EllipsoidalDistanceAttenuation ( float3 unL , float3 invHalfDim ,
float rangeAttenuationScale , float rangeAttenuationBias )
{
//#line 218
unL *= invHalfDim ;

float sqDist = dot ( unL , unL ) ;
return SmoothDistanceWindowing ( sqDist , rangeAttenuationScale , rangeAttenuationBias ) ;
}
//#line 228
float BoxDistanceAttenuation ( float3 unL , float3 invHalfDim ,
float rangeAttenuationScale , float rangeAttenuationBias )
{
float attenuation = 0.0 ;
//#line 235
unL *= invHalfDim ;
//#line 238
if ( ! ( Max3 ( abs ( unL . x ) , abs ( unL . y ) , abs ( unL . z ) ) > 1.0 ) )
{
float sqDist = ComputeCubeToSphereMapSqMagnitude ( unL ) ;
attenuation = SmoothDistanceWindowing ( sqDist , rangeAttenuationScale , rangeAttenuationBias ) ;
}
return attenuation ;
}
//#line 250
float2 GetIESTextureCoordinate ( float3x3 lightToWord , float3 L )
{

float3 dir = mul ( lightToWord , - L ) ;
//#line 256
float2 sphericalCoord ;

sphericalCoord . y = ( dir . z * 0.5 ) + 0.5 ;
float theta = atan2 ( dir . y , dir . x ) ;
sphericalCoord . x = theta * 0.15915494309189533577 ;

return sphericalCoord ;
}
//#line 270
float GetHorizonOcclusion ( float3 V , float3 normalWS , float3 vertexNormal , float horizonFade )
{
float3 R = reflect ( - V , normalWS ) ;
float specularOcclusion = saturate ( 1.0 + horizonFade * dot ( R , vertexNormal ) ) ;

return specularOcclusion * specularOcclusion ;
}
//#line 280
float GetSpecularOcclusionFromAmbientOcclusion ( float NdotV , float ambientOcclusion , float roughness )
{
return saturate ( PositivePow ( NdotV + ambientOcclusion , exp2 ( - 16.0 * roughness - 1.0 ) ) - 1.0 + ambientOcclusion ) ;
}
//#line 287
float3 GTAOMultiBounce ( float visibility , float3 albedo )
{
float3 a = 2.0404 * albedo - 0.3324 ;
float3 b = - 4.7951 * albedo + 0.6417 ;
float3 c = 2.7552 * albedo + 0.6903 ;

float x = visibility ;
return max ( x , ( ( x * a + b ) * x + c ) * x ) ;
}
//#line 299
float SphericalCapIntersectionSolidArea ( float cosC1 , float cosC2 , float cosB )
{
float r1 = FastACos ( cosC1 ) ;
float r2 = FastACos ( cosC2 ) ;
float rd = FastACos ( cosB ) ;
float area = 0.0 ;

if ( rd <= max ( r1 , r2 ) - min ( r1 , r2 ) )
{

area = 6.28318530717958647693 - 6.28318530717958647693 * max ( cosC1 , cosC2 ) ;
}
else if ( rd >= r1 + r2 )
{

area = 0.0 ;
}
else
{
float diff = abs ( r1 - r2 ) ;
float den = r1 + r2 - diff ;
float x = 1.0 - saturate ( ( rd - diff ) / max ( den , 0.0001 ) ) ;
area = smoothstep ( 0.0 , 1.0 , x ) ;
area *= 6.28318530717958647693 - 6.28318530717958647693 * max ( cosC1 , cosC2 ) ;
}

return area ;
}
//#line 331
float GetSpecularOcclusionFromBentAO_ConeCone ( float3 V , float3 bentNormalWS , float3 normalWS , float ambientOcclusion , float roughness )
{
//#line 335
float cosAv = sqrt ( 1.0 - ambientOcclusion ) ;
roughness = max ( roughness , 0.01 ) ;
float cosAs = exp2 ( ( - log ( 10.0 ) / log ( 2.0 ) ) * Sq ( roughness ) ) ;
float cosB = dot ( bentNormalWS , reflect ( - V , normalWS ) ) ;
return SphericalCapIntersectionSolidArea ( cosAv , cosAs , cosB ) / ( 6.28318530717958647693 * ( 1.0 - cosAs ) ) ;
}

float GetSpecularOcclusionFromBentAO ( float3 V , float3 bentNormalWS , float3 normalWS , float ambientOcclusion , float roughness )
{
//#line 355
float vs = - 1.0f / min ( sqrt ( 1.0f - ambientOcclusion ) - 1.0f , - 0.001f ) ;
//#line 358
float us = 0.8f ;
//#line 363
float NoV = dot ( V , normalWS ) ;
float3 NDFAxis = ( 2 * NoV * normalWS - V ) * ( 0.5f / max ( roughness * roughness * NoV , 0.001f ) ) ;

float umLength1 = length ( NDFAxis + vs * bentNormalWS ) ;
float umLength2 = length ( NDFAxis + us * normalWS ) ;
float d1 = 1 - exp ( - 2 * umLength1 ) ;
float d2 = 1 - exp ( - 2 * umLength2 ) ;

float expFactor1 = exp ( umLength1 - umLength2 + us - vs ) ;

return saturate ( expFactor1 * ( d1 * umLength2 ) / ( d2 * umLength1 ) ) ;
}
//#line 377
float ComputeWrappedDiffuseLighting ( float NdotL , float w )
{
return saturate ( ( NdotL + w ) / ( ( 1.0 + w ) * ( 1.0 + w ) ) ) ;
}
//#line 383
float3 ComputeWrappedNormal ( float3 N , float3 L , float w )
{
float NdotL = dot ( N , L ) ;
float wrappedNdotL = saturate ( ( NdotL + w ) / ( 1 + w ) ) ;
float sinPhi = lerp ( w , 0.f , wrappedNdotL ) ;
float cosPhi = sqrt ( 1.0f - sinPhi * sinPhi ) ;
return normalize ( cosPhi * N + sinPhi * cross ( cross ( N , L ) , N ) ) ;
}
//#line 393
float ComputeWrappedPowerDiffuseLighting ( float NdotL , float w , float p )
{
return pow ( saturate ( ( NdotL + w ) / ( 1.0 + w ) ) , p ) * ( p + 1 ) / ( w * 2.0 + 2.0 ) ;
}
//#line 399
float ComputeMicroShadowing ( float AO , float NdotL , float opacity )
{
float aperture = 2.0 * AO * AO ;
float microshadow = saturate ( NdotL + aperture - 1.0 ) ;
return lerp ( 1.0 , microshadow , opacity ) ;
}

float3 ComputeShadowColor ( float shadow , float3 shadowTint , float penumbraFlag )
{
//#line 413
float3 invTint = float3 ( 1.0 , 1.0 , 1.0 ) - shadowTint ;
float shadow3 = shadow * shadow * shadow ;
return lerp ( float3 ( 1.0 , 1.0 , 1.0 ) - ( ( 1.0 - shadow ) * invTint )
, shadow3 * invTint + shadow * shadowTint ,
penumbraFlag ) ;

}
//#line 422
float3 ComputeShadowColor ( float3 shadow , float3 shadowTint , float penumbraFlag )
{
//#line 429
float3 invTint = float3 ( 1.0 , 1.0 , 1.0 ) - shadowTint ;
float3 shadow3 = shadow * shadow * shadow ;
return lerp ( float3 ( 1.0 , 1.0 , 1.0 ) - ( ( 1.0 - shadow ) * invTint )
, shadow3 * invTint + shadow * shadowTint ,
penumbraFlag ) ;

}
//#line 442
float ClampNdotV ( float NdotV )
{
return max ( NdotV , 0.0001 ) ;
}
//#line 449
void GetBSDFAngle ( float3 V , float3 L , float NdotL , float NdotV ,
out float LdotV , out float NdotH , out float LdotH , out float invLenLV )
{

LdotV = dot ( L , V ) ;
invLenLV = rsqrt ( max ( 2.0 * LdotV + 2.0 , 5.960464478e-8 ) ) ;
NdotH = saturate ( ( NdotL + NdotV ) * invLenLV ) ;
LdotH = saturate ( invLenLV * LdotV + invLenLV ) ;
}
//#line 462
float3 GetViewReflectedNormal ( float3 N , float3 V , out float NdotV )
{
//#line 489
NdotV = dot ( N , V ) ;
//#line 492
N += ( 2.0 * saturate ( - NdotV ) ) * V ;
NdotV = abs ( NdotV ) ;

return N ;
}
//#line 501
float3x3 GetLocalFrame ( float3 localZ )
{
float x = localZ . x ;
float y = localZ . y ;
float z = localZ . z ;
float sz = FastSign ( z ) ;
float a = 1 / ( sz + z ) ;
float ya = y * a ;
float b = x * ya ;
float c = x * sz ;

float3 localX = float3 ( c * x * a - 1 , sz * b , c ) ;
float3 localY = float3 ( b , y * ya - sz , y ) ;
//#line 517
return float3x3 ( localX , localY , localZ ) ;
}
//#line 522
float3x3 GetLocalFrame ( float3 localZ , float3 localX )
{
float3 localY = cross ( localZ , localX ) ;

return float3x3 ( localX , localY , localZ ) ;
}
//#line 531
float3x3 GetOrthoBasisViewNormal ( float3 V , float3 N , float unclampedNdotV , bool testSingularity = true )
{
float3x3 orthoBasisViewNormal ;
if ( testSingularity && ( abs ( 1.0 - unclampedNdotV ) <= 5.960464478e-8 ) )
{
//#line 538
orthoBasisViewNormal = GetLocalFrame ( N ) ;
}
else
{
orthoBasisViewNormal [ 0 ] = normalize ( V - N * unclampedNdotV ) ;
orthoBasisViewNormal [ 2 ] = N ;
orthoBasisViewNormal [ 1 ] = cross ( orthoBasisViewNormal [ 2 ] , orthoBasisViewNormal [ 0 ] ) ;
}
return orthoBasisViewNormal ;
}
//#line 550
bool IsMatchingLightLayer ( uint lightLayers , uint renderingLayers )
{
return ( lightLayers & renderingLayers ) != 0 ;
}
//#line 11 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/Sampling/Fibonacci.hlsl"
float2 Fibonacci2dSeq ( float fibN1 , float fibN2 , uint i )
{
//#line 16
return float2 ( i / fibN1 + ( 0.5 / fibN1 ) , frac ( i * ( fibN2 / fibN1 ) ) ) ;
}
//#line 23
float2 Golden2dSeq ( uint i , float n )
{
//#line 27
return float2 ( i / n + ( 0.5 / n ) , frac ( i * rcp ( 1.618033988749895 ) ) ) ;
}

static const uint k_FibonacciSeq [ ] = {
0 , 1 , 1 , 2 , 3 , 5 , 8 , 13 , 21 , 34 , 55 , 89 , 144 , 233 , 377 , 610 , 987 , 1597 , 2584 , 4181
} ;

static const float2 k_Fibonacci2dSeq21 [ ] = {
float2 ( 0.02380952 , 0.00000000 ) ,
float2 ( 0.07142857 , 0.61904764 ) ,
float2 ( 0.11904762 , 0.23809528 ) ,
float2 ( 0.16666667 , 0.85714293 ) ,
float2 ( 0.21428572 , 0.47619057 ) ,
float2 ( 0.26190478 , 0.09523821 ) ,
float2 ( 0.30952382 , 0.71428585 ) ,
float2 ( 0.35714287 , 0.33333349 ) ,
float2 ( 0.40476191 , 0.95238113 ) ,
float2 ( 0.45238096 , 0.57142878 ) ,
float2 ( 0.50000000 , 0.19047642 ) ,
float2 ( 0.54761904 , 0.80952406 ) ,
float2 ( 0.59523809 , 0.42857170 ) ,
float2 ( 0.64285713 , 0.04761887 ) ,
float2 ( 0.69047618 , 0.66666698 ) ,
float2 ( 0.73809522 , 0.28571510 ) ,
float2 ( 0.78571427 , 0.90476227 ) ,
float2 ( 0.83333331 , 0.52380943 ) ,
float2 ( 0.88095236 , 0.14285755 ) ,
float2 ( 0.92857140 , 0.76190567 ) ,
float2 ( 0.97619045 , 0.38095284 )
} ;

static const float2 k_Fibonacci2dSeq34 [ ] = {
float2 ( 0.01470588 , 0.00000000 ) ,
float2 ( 0.04411765 , 0.61764705 ) ,
float2 ( 0.07352941 , 0.23529410 ) ,
float2 ( 0.10294118 , 0.85294116 ) ,
float2 ( 0.13235295 , 0.47058821 ) ,
float2 ( 0.16176471 , 0.08823538 ) ,
float2 ( 0.19117647 , 0.70588231 ) ,
float2 ( 0.22058824 , 0.32352924 ) ,
float2 ( 0.25000000 , 0.94117641 ) ,
float2 ( 0.27941176 , 0.55882359 ) ,
float2 ( 0.30882353 , 0.17647076 ) ,
float2 ( 0.33823529 , 0.79411745 ) ,
float2 ( 0.36764705 , 0.41176462 ) ,
float2 ( 0.39705881 , 0.02941132 ) ,
float2 ( 0.42647058 , 0.64705849 ) ,
float2 ( 0.45588234 , 0.26470566 ) ,
float2 ( 0.48529410 , 0.88235283 ) ,
float2 ( 0.51470590 , 0.50000000 ) ,
float2 ( 0.54411763 , 0.11764717 ) ,
float2 ( 0.57352942 , 0.73529434 ) ,
float2 ( 0.60294116 , 0.35294151 ) ,
float2 ( 0.63235295 , 0.97058773 ) ,
float2 ( 0.66176468 , 0.58823490 ) ,
float2 ( 0.69117647 , 0.20588207 ) ,
float2 ( 0.72058821 , 0.82352924 ) ,
float2 ( 0.75000000 , 0.44117641 ) ,
float2 ( 0.77941179 , 0.05882263 ) ,
float2 ( 0.80882353 , 0.67646980 ) ,
float2 ( 0.83823532 , 0.29411697 ) ,
float2 ( 0.86764705 , 0.91176414 ) ,
float2 ( 0.89705884 , 0.52941132 ) ,
float2 ( 0.92647058 , 0.14705849 ) ,
float2 ( 0.95588237 , 0.76470566 ) ,
float2 ( 0.98529410 , 0.38235283 )
} ;

static const float2 k_Fibonacci2dSeq55 [ ] = {
float2 ( 0.00909091 , 0.00000000 ) ,
float2 ( 0.02727273 , 0.61818182 ) ,
float2 ( 0.04545455 , 0.23636365 ) ,
float2 ( 0.06363636 , 0.85454547 ) ,
float2 ( 0.08181818 , 0.47272730 ) ,
float2 ( 0.10000000 , 0.09090900 ) ,
float2 ( 0.11818182 , 0.70909095 ) ,
float2 ( 0.13636364 , 0.32727289 ) ,
float2 ( 0.15454546 , 0.94545460 ) ,
float2 ( 0.17272727 , 0.56363630 ) ,
float2 ( 0.19090909 , 0.18181801 ) ,
float2 ( 0.20909090 , 0.80000019 ) ,
float2 ( 0.22727273 , 0.41818190 ) ,
float2 ( 0.24545455 , 0.03636360 ) ,
float2 ( 0.26363635 , 0.65454578 ) ,
float2 ( 0.28181818 , 0.27272701 ) ,
float2 ( 0.30000001 , 0.89090919 ) ,
float2 ( 0.31818181 , 0.50909138 ) ,
float2 ( 0.33636364 , 0.12727261 ) ,
float2 ( 0.35454544 , 0.74545479 ) ,
float2 ( 0.37272727 , 0.36363602 ) ,
float2 ( 0.39090911 , 0.98181820 ) ,
float2 ( 0.40909091 , 0.60000038 ) ,
float2 ( 0.42727274 , 0.21818161 ) ,
float2 ( 0.44545454 , 0.83636379 ) ,
float2 ( 0.46363637 , 0.45454597 ) ,
float2 ( 0.48181817 , 0.07272720 ) ,
float2 ( 0.50000000 , 0.69090843 ) ,
float2 ( 0.51818180 , 0.30909157 ) ,
float2 ( 0.53636366 , 0.92727280 ) ,
float2 ( 0.55454546 , 0.54545403 ) ,
float2 ( 0.57272726 , 0.16363716 ) ,
float2 ( 0.59090906 , 0.78181839 ) ,
float2 ( 0.60909092 , 0.39999962 ) ,
float2 ( 0.62727273 , 0.01818275 ) ,
float2 ( 0.64545453 , 0.63636398 ) ,
float2 ( 0.66363639 , 0.25454521 ) ,
float2 ( 0.68181819 , 0.87272835 ) ,
float2 ( 0.69999999 , 0.49090958 ) ,
float2 ( 0.71818179 , 0.10909081 ) ,
float2 ( 0.73636365 , 0.72727203 ) ,
float2 ( 0.75454545 , 0.34545517 ) ,
float2 ( 0.77272725 , 0.96363640 ) ,
float2 ( 0.79090911 , 0.58181763 ) ,
float2 ( 0.80909091 , 0.20000076 ) ,
float2 ( 0.82727271 , 0.81818199 ) ,
float2 ( 0.84545457 , 0.43636322 ) ,
float2 ( 0.86363637 , 0.05454636 ) ,
float2 ( 0.88181818 , 0.67272758 ) ,
float2 ( 0.89999998 , 0.29090881 ) ,
float2 ( 0.91818184 , 0.90909195 ) ,
float2 ( 0.93636364 , 0.52727318 ) ,
float2 ( 0.95454544 , 0.14545441 ) ,
float2 ( 0.97272730 , 0.76363754 ) ,
float2 ( 0.99090910 , 0.38181686 )
} ;

static const float2 k_Fibonacci2dSeq89 [ ] = {
float2 ( 0.00561798 , 0.00000000 ) ,
float2 ( 0.01685393 , 0.61797750 ) ,
float2 ( 0.02808989 , 0.23595500 ) ,
float2 ( 0.03932584 , 0.85393250 ) ,
float2 ( 0.05056180 , 0.47191000 ) ,
float2 ( 0.06179775 , 0.08988762 ) ,
float2 ( 0.07303371 , 0.70786500 ) ,
float2 ( 0.08426967 , 0.32584238 ) ,
float2 ( 0.09550562 , 0.94382000 ) ,
float2 ( 0.10674157 , 0.56179762 ) ,
float2 ( 0.11797753 , 0.17977524 ) ,
float2 ( 0.12921348 , 0.79775238 ) ,
float2 ( 0.14044943 , 0.41573000 ) ,
float2 ( 0.15168539 , 0.03370762 ) ,
float2 ( 0.16292135 , 0.65168476 ) ,
float2 ( 0.17415731 , 0.26966286 ) ,
float2 ( 0.18539326 , 0.88764000 ) ,
float2 ( 0.19662921 , 0.50561714 ) ,
float2 ( 0.20786516 , 0.12359524 ) ,
float2 ( 0.21910113 , 0.74157238 ) ,
float2 ( 0.23033708 , 0.35955048 ) ,
float2 ( 0.24157304 , 0.97752762 ) ,
float2 ( 0.25280899 , 0.59550476 ) ,
float2 ( 0.26404494 , 0.21348286 ) ,
float2 ( 0.27528089 , 0.83146000 ) ,
float2 ( 0.28651685 , 0.44943714 ) ,
float2 ( 0.29775280 , 0.06741524 ) ,
float2 ( 0.30898875 , 0.68539238 ) ,
float2 ( 0.32022473 , 0.30336952 ) ,
float2 ( 0.33146068 , 0.92134666 ) ,
float2 ( 0.34269664 , 0.53932571 ) ,
float2 ( 0.35393259 , 0.15730286 ) ,
float2 ( 0.36516854 , 0.77528000 ) ,
float2 ( 0.37640449 , 0.39325714 ) ,
float2 ( 0.38764045 , 0.01123428 ) ,
float2 ( 0.39887640 , 0.62921333 ) ,
float2 ( 0.41011235 , 0.24719048 ) ,
float2 ( 0.42134830 , 0.86516762 ) ,
float2 ( 0.43258426 , 0.48314476 ) ,
float2 ( 0.44382024 , 0.10112190 ) ,
float2 ( 0.45505619 , 0.71910095 ) ,
float2 ( 0.46629214 , 0.33707809 ) ,
float2 ( 0.47752810 , 0.95505524 ) ,
float2 ( 0.48876405 , 0.57303238 ) ,
float2 ( 0.50000000 , 0.19100952 ) ,
float2 ( 0.51123595 , 0.80898666 ) ,
float2 ( 0.52247190 , 0.42696571 ) ,
float2 ( 0.53370786 , 0.04494286 ) ,
float2 ( 0.54494381 , 0.66292000 ) ,
float2 ( 0.55617976 , 0.28089714 ) ,
float2 ( 0.56741571 , 0.89887428 ) ,
float2 ( 0.57865167 , 0.51685333 ) ,
float2 ( 0.58988762 , 0.13483047 ) ,
float2 ( 0.60112357 , 0.75280762 ) ,
float2 ( 0.61235952 , 0.37078476 ) ,
float2 ( 0.62359548 , 0.98876190 ) ,
float2 ( 0.63483149 , 0.60673904 ) ,
float2 ( 0.64606744 , 0.22471619 ) ,
float2 ( 0.65730339 , 0.84269333 ) ,
float2 ( 0.66853935 , 0.46067429 ) ,
float2 ( 0.67977530 , 0.07865143 ) ,
float2 ( 0.69101125 , 0.69662857 ) ,
float2 ( 0.70224720 , 0.31460571 ) ,
float2 ( 0.71348315 , 0.93258286 ) ,
float2 ( 0.72471911 , 0.55056000 ) ,
float2 ( 0.73595506 , 0.16853714 ) ,
float2 ( 0.74719101 , 0.78651428 ) ,
float2 ( 0.75842696 , 0.40449142 ) ,
float2 ( 0.76966292 , 0.02246857 ) ,
float2 ( 0.78089887 , 0.64044571 ) ,
float2 ( 0.79213482 , 0.25842667 ) ,
float2 ( 0.80337077 , 0.87640381 ) ,
float2 ( 0.81460673 , 0.49438095 ) ,
float2 ( 0.82584268 , 0.11235809 ) ,
float2 ( 0.83707863 , 0.73033524 ) ,
float2 ( 0.84831458 , 0.34831238 ) ,
float2 ( 0.85955054 , 0.96628952 ) ,
float2 ( 0.87078649 , 0.58426666 ) ,
float2 ( 0.88202250 , 0.20224380 ) ,
float2 ( 0.89325845 , 0.82022095 ) ,
float2 ( 0.90449440 , 0.43820190 ) ,
float2 ( 0.91573036 , 0.05617905 ) ,
float2 ( 0.92696631 , 0.67415619 ) ,
float2 ( 0.93820226 , 0.29213333 ) ,
float2 ( 0.94943821 , 0.91011047 ) ,
float2 ( 0.96067417 , 0.52808762 ) ,
float2 ( 0.97191012 , 0.14606476 ) ,
float2 ( 0.98314607 , 0.76404190 ) ,
float2 ( 0.99438202 , 0.38201904 )
} ;
//#line 248
float2 Fibonacci2d ( uint i , uint sampleCount )
{
switch ( sampleCount )
{
case 21 : return k_Fibonacci2dSeq21 [ i ] ;
case 34 : return k_Fibonacci2dSeq34 [ i ] ;
case 55 : return k_Fibonacci2dSeq55 [ i ] ;
case 89 : return k_Fibonacci2dSeq89 [ i ] ;
default :
{
uint fibN1 = sampleCount ;
uint fibN2 = sampleCount ;
//#line 262
for ( uint j = 1 ; j < 20 ; j ++ )
{
if ( k_FibonacciSeq [ j ] == fibN1 )
{
fibN2 = k_FibonacciSeq [ j - 1 ] ;
}
}

return Fibonacci2dSeq ( fibN1 , fibN2 , i ) ;
}
}
}

float2 SampleDiskGolden ( uint i , uint sampleCount )
{
float2 f = Golden2dSeq ( i , sampleCount ) ;
return float2 ( sqrt ( f . x ) , 6.28318530717958647693 * f . y ) ;
}
//#line 282
float2 SampleDiskFibonacci ( uint i , uint sampleCount )
{
float2 f = Fibonacci2d ( i , sampleCount ) ;
return float2 ( sqrt ( f . x ) , 6.28318530717958647693 * f . y ) ;
}
//#line 289
float2 SampleHemisphereFibonacci ( uint i , uint sampleCount )
{
float2 f = Fibonacci2d ( i , sampleCount ) ;
return float2 ( 1 - f . x , 6.28318530717958647693 * f . y ) ;
}
//#line 296
float2 SampleSphereFibonacci ( uint i , uint sampleCount )
{
float2 f = Fibonacci2d ( i , sampleCount ) ;
return float2 ( 1 - 2 * f . x , 6.28318530717958647693 * f . y ) ;
}
//#line 9 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/Sampling/Hammersley.hlsl"
uint ReverseBits32 ( uint bits )
{

return reversebits ( bits ) ;
//#line 21
}

float VanDerCorputBase2 ( uint i )
{
return ReverseBits32 ( i ) * rcp ( 4294967296.0 ) ;
}

float2 Hammersley2dSeq ( uint i , uint sequenceLength )
{
return float2 ( float ( i ) / float ( sequenceLength ) , VanDerCorputBase2 ( i ) ) ;
}
//#line 37
static const float2 k_Hammersley2dSeq16 [ ] = {
float2 ( 0.00000000 , 0.00000000 ) ,
float2 ( 0.06250000 , 0.50000000 ) ,
float2 ( 0.12500000 , 0.25000000 ) ,
float2 ( 0.18750000 , 0.75000000 ) ,
float2 ( 0.25000000 , 0.12500000 ) ,
float2 ( 0.31250000 , 0.62500000 ) ,
float2 ( 0.37500000 , 0.37500000 ) ,
float2 ( 0.43750000 , 0.87500000 ) ,
float2 ( 0.50000000 , 0.06250000 ) ,
float2 ( 0.56250000 , 0.56250000 ) ,
float2 ( 0.62500000 , 0.31250000 ) ,
float2 ( 0.68750000 , 0.81250000 ) ,
float2 ( 0.75000000 , 0.18750000 ) ,
float2 ( 0.81250000 , 0.68750000 ) ,
float2 ( 0.87500000 , 0.43750000 ) ,
float2 ( 0.93750000 , 0.93750000 )
} ;

static const float2 k_Hammersley2dSeq32 [ ] = {
float2 ( 0.00000000 , 0.00000000 ) ,
float2 ( 0.03125000 , 0.50000000 ) ,
float2 ( 0.06250000 , 0.25000000 ) ,
float2 ( 0.09375000 , 0.75000000 ) ,
float2 ( 0.12500000 , 0.12500000 ) ,
float2 ( 0.15625000 , 0.62500000 ) ,
float2 ( 0.18750000 , 0.37500000 ) ,
float2 ( 0.21875000 , 0.87500000 ) ,
float2 ( 0.25000000 , 0.06250000 ) ,
float2 ( 0.28125000 , 0.56250000 ) ,
float2 ( 0.31250000 , 0.31250000 ) ,
float2 ( 0.34375000 , 0.81250000 ) ,
float2 ( 0.37500000 , 0.18750000 ) ,
float2 ( 0.40625000 , 0.68750000 ) ,
float2 ( 0.43750000 , 0.43750000 ) ,
float2 ( 0.46875000 , 0.93750000 ) ,
float2 ( 0.50000000 , 0.03125000 ) ,
float2 ( 0.53125000 , 0.53125000 ) ,
float2 ( 0.56250000 , 0.28125000 ) ,
float2 ( 0.59375000 , 0.78125000 ) ,
float2 ( 0.62500000 , 0.15625000 ) ,
float2 ( 0.65625000 , 0.65625000 ) ,
float2 ( 0.68750000 , 0.40625000 ) ,
float2 ( 0.71875000 , 0.90625000 ) ,
float2 ( 0.75000000 , 0.09375000 ) ,
float2 ( 0.78125000 , 0.59375000 ) ,
float2 ( 0.81250000 , 0.34375000 ) ,
float2 ( 0.84375000 , 0.84375000 ) ,
float2 ( 0.87500000 , 0.21875000 ) ,
float2 ( 0.90625000 , 0.71875000 ) ,
float2 ( 0.93750000 , 0.46875000 ) ,
float2 ( 0.96875000 , 0.96875000 )
} ;

static const float2 k_Hammersley2dSeq64 [ ] = {
float2 ( 0.00000000 , 0.00000000 ) ,
float2 ( 0.01562500 , 0.50000000 ) ,
float2 ( 0.03125000 , 0.25000000 ) ,
float2 ( 0.04687500 , 0.75000000 ) ,
float2 ( 0.06250000 , 0.12500000 ) ,
float2 ( 0.07812500 , 0.62500000 ) ,
float2 ( 0.09375000 , 0.37500000 ) ,
float2 ( 0.10937500 , 0.87500000 ) ,
float2 ( 0.12500000 , 0.06250000 ) ,
float2 ( 0.14062500 , 0.56250000 ) ,
float2 ( 0.15625000 , 0.31250000 ) ,
float2 ( 0.17187500 , 0.81250000 ) ,
float2 ( 0.18750000 , 0.18750000 ) ,
float2 ( 0.20312500 , 0.68750000 ) ,
float2 ( 0.21875000 , 0.43750000 ) ,
float2 ( 0.23437500 , 0.93750000 ) ,
float2 ( 0.25000000 , 0.03125000 ) ,
float2 ( 0.26562500 , 0.53125000 ) ,
float2 ( 0.28125000 , 0.28125000 ) ,
float2 ( 0.29687500 , 0.78125000 ) ,
float2 ( 0.31250000 , 0.15625000 ) ,
float2 ( 0.32812500 , 0.65625000 ) ,
float2 ( 0.34375000 , 0.40625000 ) ,
float2 ( 0.35937500 , 0.90625000 ) ,
float2 ( 0.37500000 , 0.09375000 ) ,
float2 ( 0.39062500 , 0.59375000 ) ,
float2 ( 0.40625000 , 0.34375000 ) ,
float2 ( 0.42187500 , 0.84375000 ) ,
float2 ( 0.43750000 , 0.21875000 ) ,
float2 ( 0.45312500 , 0.71875000 ) ,
float2 ( 0.46875000 , 0.46875000 ) ,
float2 ( 0.48437500 , 0.96875000 ) ,
float2 ( 0.50000000 , 0.01562500 ) ,
float2 ( 0.51562500 , 0.51562500 ) ,
float2 ( 0.53125000 , 0.26562500 ) ,
float2 ( 0.54687500 , 0.76562500 ) ,
float2 ( 0.56250000 , 0.14062500 ) ,
float2 ( 0.57812500 , 0.64062500 ) ,
float2 ( 0.59375000 , 0.39062500 ) ,
float2 ( 0.60937500 , 0.89062500 ) ,
float2 ( 0.62500000 , 0.07812500 ) ,
float2 ( 0.64062500 , 0.57812500 ) ,
float2 ( 0.65625000 , 0.32812500 ) ,
float2 ( 0.67187500 , 0.82812500 ) ,
float2 ( 0.68750000 , 0.20312500 ) ,
float2 ( 0.70312500 , 0.70312500 ) ,
float2 ( 0.71875000 , 0.45312500 ) ,
float2 ( 0.73437500 , 0.95312500 ) ,
float2 ( 0.75000000 , 0.04687500 ) ,
float2 ( 0.76562500 , 0.54687500 ) ,
float2 ( 0.78125000 , 0.29687500 ) ,
float2 ( 0.79687500 , 0.79687500 ) ,
float2 ( 0.81250000 , 0.17187500 ) ,
float2 ( 0.82812500 , 0.67187500 ) ,
float2 ( 0.84375000 , 0.42187500 ) ,
float2 ( 0.85937500 , 0.92187500 ) ,
float2 ( 0.87500000 , 0.10937500 ) ,
float2 ( 0.89062500 , 0.60937500 ) ,
float2 ( 0.90625000 , 0.35937500 ) ,
float2 ( 0.92187500 , 0.85937500 ) ,
float2 ( 0.93750000 , 0.23437500 ) ,
float2 ( 0.95312500 , 0.73437500 ) ,
float2 ( 0.96875000 , 0.48437500 ) ,
float2 ( 0.98437500 , 0.98437500 )
} ;

static const float2 k_Hammersley2dSeq256 [ ] = {
float2 ( 0.00000000 , 0.00000000 ) ,
float2 ( 0.00390625 , 0.50000000 ) ,
float2 ( 0.00781250 , 0.25000000 ) ,
float2 ( 0.01171875 , 0.75000000 ) ,
float2 ( 0.01562500 , 0.12500000 ) ,
float2 ( 0.01953125 , 0.62500000 ) ,
float2 ( 0.02343750 , 0.37500000 ) ,
float2 ( 0.02734375 , 0.87500000 ) ,
float2 ( 0.03125000 , 0.06250000 ) ,
float2 ( 0.03515625 , 0.56250000 ) ,
float2 ( 0.03906250 , 0.31250000 ) ,
float2 ( 0.04296875 , 0.81250000 ) ,
float2 ( 0.04687500 , 0.18750000 ) ,
float2 ( 0.05078125 , 0.68750000 ) ,
float2 ( 0.05468750 , 0.43750000 ) ,
float2 ( 0.05859375 , 0.93750000 ) ,
float2 ( 0.06250000 , 0.03125000 ) ,
float2 ( 0.06640625 , 0.53125000 ) ,
float2 ( 0.07031250 , 0.28125000 ) ,
float2 ( 0.07421875 , 0.78125000 ) ,
float2 ( 0.07812500 , 0.15625000 ) ,
float2 ( 0.08203125 , 0.65625000 ) ,
float2 ( 0.08593750 , 0.40625000 ) ,
float2 ( 0.08984375 , 0.90625000 ) ,
float2 ( 0.09375000 , 0.09375000 ) ,
float2 ( 0.09765625 , 0.59375000 ) ,
float2 ( 0.10156250 , 0.34375000 ) ,
float2 ( 0.10546875 , 0.84375000 ) ,
float2 ( 0.10937500 , 0.21875000 ) ,
float2 ( 0.11328125 , 0.71875000 ) ,
float2 ( 0.11718750 , 0.46875000 ) ,
float2 ( 0.12109375 , 0.96875000 ) ,
float2 ( 0.12500000 , 0.01562500 ) ,
float2 ( 0.12890625 , 0.51562500 ) ,
float2 ( 0.13281250 , 0.26562500 ) ,
float2 ( 0.13671875 , 0.76562500 ) ,
float2 ( 0.14062500 , 0.14062500 ) ,
float2 ( 0.14453125 , 0.64062500 ) ,
float2 ( 0.14843750 , 0.39062500 ) ,
float2 ( 0.15234375 , 0.89062500 ) ,
float2 ( 0.15625000 , 0.07812500 ) ,
float2 ( 0.16015625 , 0.57812500 ) ,
float2 ( 0.16406250 , 0.32812500 ) ,
float2 ( 0.16796875 , 0.82812500 ) ,
float2 ( 0.17187500 , 0.20312500 ) ,
float2 ( 0.17578125 , 0.70312500 ) ,
float2 ( 0.17968750 , 0.45312500 ) ,
float2 ( 0.18359375 , 0.95312500 ) ,
float2 ( 0.18750000 , 0.04687500 ) ,
float2 ( 0.19140625 , 0.54687500 ) ,
float2 ( 0.19531250 , 0.29687500 ) ,
float2 ( 0.19921875 , 0.79687500 ) ,
float2 ( 0.20312500 , 0.17187500 ) ,
float2 ( 0.20703125 , 0.67187500 ) ,
float2 ( 0.21093750 , 0.42187500 ) ,
float2 ( 0.21484375 , 0.92187500 ) ,
float2 ( 0.21875000 , 0.10937500 ) ,
float2 ( 0.22265625 , 0.60937500 ) ,
float2 ( 0.22656250 , 0.35937500 ) ,
float2 ( 0.23046875 , 0.85937500 ) ,
float2 ( 0.23437500 , 0.23437500 ) ,
float2 ( 0.23828125 , 0.73437500 ) ,
float2 ( 0.24218750 , 0.48437500 ) ,
float2 ( 0.24609375 , 0.98437500 ) ,
float2 ( 0.25000000 , 0.00781250 ) ,
float2 ( 0.25390625 , 0.50781250 ) ,
float2 ( 0.25781250 , 0.25781250 ) ,
float2 ( 0.26171875 , 0.75781250 ) ,
float2 ( 0.26562500 , 0.13281250 ) ,
float2 ( 0.26953125 , 0.63281250 ) ,
float2 ( 0.27343750 , 0.38281250 ) ,
float2 ( 0.27734375 , 0.88281250 ) ,
float2 ( 0.28125000 , 0.07031250 ) ,
float2 ( 0.28515625 , 0.57031250 ) ,
float2 ( 0.28906250 , 0.32031250 ) ,
float2 ( 0.29296875 , 0.82031250 ) ,
float2 ( 0.29687500 , 0.19531250 ) ,
float2 ( 0.30078125 , 0.69531250 ) ,
float2 ( 0.30468750 , 0.44531250 ) ,
float2 ( 0.30859375 , 0.94531250 ) ,
float2 ( 0.31250000 , 0.03906250 ) ,
float2 ( 0.31640625 , 0.53906250 ) ,
float2 ( 0.32031250 , 0.28906250 ) ,
float2 ( 0.32421875 , 0.78906250 ) ,
float2 ( 0.32812500 , 0.16406250 ) ,
float2 ( 0.33203125 , 0.66406250 ) ,
float2 ( 0.33593750 , 0.41406250 ) ,
float2 ( 0.33984375 , 0.91406250 ) ,
float2 ( 0.34375000 , 0.10156250 ) ,
float2 ( 0.34765625 , 0.60156250 ) ,
float2 ( 0.35156250 , 0.35156250 ) ,
float2 ( 0.35546875 , 0.85156250 ) ,
float2 ( 0.35937500 , 0.22656250 ) ,
float2 ( 0.36328125 , 0.72656250 ) ,
float2 ( 0.36718750 , 0.47656250 ) ,
float2 ( 0.37109375 , 0.97656250 ) ,
float2 ( 0.37500000 , 0.02343750 ) ,
float2 ( 0.37890625 , 0.52343750 ) ,
float2 ( 0.38281250 , 0.27343750 ) ,
float2 ( 0.38671875 , 0.77343750 ) ,
float2 ( 0.39062500 , 0.14843750 ) ,
float2 ( 0.39453125 , 0.64843750 ) ,
float2 ( 0.39843750 , 0.39843750 ) ,
float2 ( 0.40234375 , 0.89843750 ) ,
float2 ( 0.40625000 , 0.08593750 ) ,
float2 ( 0.41015625 , 0.58593750 ) ,
float2 ( 0.41406250 , 0.33593750 ) ,
float2 ( 0.41796875 , 0.83593750 ) ,
float2 ( 0.42187500 , 0.21093750 ) ,
float2 ( 0.42578125 , 0.71093750 ) ,
float2 ( 0.42968750 , 0.46093750 ) ,
float2 ( 0.43359375 , 0.96093750 ) ,
float2 ( 0.43750000 , 0.05468750 ) ,
float2 ( 0.44140625 , 0.55468750 ) ,
float2 ( 0.44531250 , 0.30468750 ) ,
float2 ( 0.44921875 , 0.80468750 ) ,
float2 ( 0.45312500 , 0.17968750 ) ,
float2 ( 0.45703125 , 0.67968750 ) ,
float2 ( 0.46093750 , 0.42968750 ) ,
float2 ( 0.46484375 , 0.92968750 ) ,
float2 ( 0.46875000 , 0.11718750 ) ,
float2 ( 0.47265625 , 0.61718750 ) ,
float2 ( 0.47656250 , 0.36718750 ) ,
float2 ( 0.48046875 , 0.86718750 ) ,
float2 ( 0.48437500 , 0.24218750 ) ,
float2 ( 0.48828125 , 0.74218750 ) ,
float2 ( 0.49218750 , 0.49218750 ) ,
float2 ( 0.49609375 , 0.99218750 ) ,
float2 ( 0.50000000 , 0.00390625 ) ,
float2 ( 0.50390625 , 0.50390625 ) ,
float2 ( 0.50781250 , 0.25390625 ) ,
float2 ( 0.51171875 , 0.75390625 ) ,
float2 ( 0.51562500 , 0.12890625 ) ,
float2 ( 0.51953125 , 0.62890625 ) ,
float2 ( 0.52343750 , 0.37890625 ) ,
float2 ( 0.52734375 , 0.87890625 ) ,
float2 ( 0.53125000 , 0.06640625 ) ,
float2 ( 0.53515625 , 0.56640625 ) ,
float2 ( 0.53906250 , 0.31640625 ) ,
float2 ( 0.54296875 , 0.81640625 ) ,
float2 ( 0.54687500 , 0.19140625 ) ,
float2 ( 0.55078125 , 0.69140625 ) ,
float2 ( 0.55468750 , 0.44140625 ) ,
float2 ( 0.55859375 , 0.94140625 ) ,
float2 ( 0.56250000 , 0.03515625 ) ,
float2 ( 0.56640625 , 0.53515625 ) ,
float2 ( 0.57031250 , 0.28515625 ) ,
float2 ( 0.57421875 , 0.78515625 ) ,
float2 ( 0.57812500 , 0.16015625 ) ,
float2 ( 0.58203125 , 0.66015625 ) ,
float2 ( 0.58593750 , 0.41015625 ) ,
float2 ( 0.58984375 , 0.91015625 ) ,
float2 ( 0.59375000 , 0.09765625 ) ,
float2 ( 0.59765625 , 0.59765625 ) ,
float2 ( 0.60156250 , 0.34765625 ) ,
float2 ( 0.60546875 , 0.84765625 ) ,
float2 ( 0.60937500 , 0.22265625 ) ,
float2 ( 0.61328125 , 0.72265625 ) ,
float2 ( 0.61718750 , 0.47265625 ) ,
float2 ( 0.62109375 , 0.97265625 ) ,
float2 ( 0.62500000 , 0.01953125 ) ,
float2 ( 0.62890625 , 0.51953125 ) ,
float2 ( 0.63281250 , 0.26953125 ) ,
float2 ( 0.63671875 , 0.76953125 ) ,
float2 ( 0.64062500 , 0.14453125 ) ,
float2 ( 0.64453125 , 0.64453125 ) ,
float2 ( 0.64843750 , 0.39453125 ) ,
float2 ( 0.65234375 , 0.89453125 ) ,
float2 ( 0.65625000 , 0.08203125 ) ,
float2 ( 0.66015625 , 0.58203125 ) ,
float2 ( 0.66406250 , 0.33203125 ) ,
float2 ( 0.66796875 , 0.83203125 ) ,
float2 ( 0.67187500 , 0.20703125 ) ,
float2 ( 0.67578125 , 0.70703125 ) ,
float2 ( 0.67968750 , 0.45703125 ) ,
float2 ( 0.68359375 , 0.95703125 ) ,
float2 ( 0.68750000 , 0.05078125 ) ,
float2 ( 0.69140625 , 0.55078125 ) ,
float2 ( 0.69531250 , 0.30078125 ) ,
float2 ( 0.69921875 , 0.80078125 ) ,
float2 ( 0.70312500 , 0.17578125 ) ,
float2 ( 0.70703125 , 0.67578125 ) ,
float2 ( 0.71093750 , 0.42578125 ) ,
float2 ( 0.71484375 , 0.92578125 ) ,
float2 ( 0.71875000 , 0.11328125 ) ,
float2 ( 0.72265625 , 0.61328125 ) ,
float2 ( 0.72656250 , 0.36328125 ) ,
float2 ( 0.73046875 , 0.86328125 ) ,
float2 ( 0.73437500 , 0.23828125 ) ,
float2 ( 0.73828125 , 0.73828125 ) ,
float2 ( 0.74218750 , 0.48828125 ) ,
float2 ( 0.74609375 , 0.98828125 ) ,
float2 ( 0.75000000 , 0.01171875 ) ,
float2 ( 0.75390625 , 0.51171875 ) ,
float2 ( 0.75781250 , 0.26171875 ) ,
float2 ( 0.76171875 , 0.76171875 ) ,
float2 ( 0.76562500 , 0.13671875 ) ,
float2 ( 0.76953125 , 0.63671875 ) ,
float2 ( 0.77343750 , 0.38671875 ) ,
float2 ( 0.77734375 , 0.88671875 ) ,
float2 ( 0.78125000 , 0.07421875 ) ,
float2 ( 0.78515625 , 0.57421875 ) ,
float2 ( 0.78906250 , 0.32421875 ) ,
float2 ( 0.79296875 , 0.82421875 ) ,
float2 ( 0.79687500 , 0.19921875 ) ,
float2 ( 0.80078125 , 0.69921875 ) ,
float2 ( 0.80468750 , 0.44921875 ) ,
float2 ( 0.80859375 , 0.94921875 ) ,
float2 ( 0.81250000 , 0.04296875 ) ,
float2 ( 0.81640625 , 0.54296875 ) ,
float2 ( 0.82031250 , 0.29296875 ) ,
float2 ( 0.82421875 , 0.79296875 ) ,
float2 ( 0.82812500 , 0.16796875 ) ,
float2 ( 0.83203125 , 0.66796875 ) ,
float2 ( 0.83593750 , 0.41796875 ) ,
float2 ( 0.83984375 , 0.91796875 ) ,
float2 ( 0.84375000 , 0.10546875 ) ,
float2 ( 0.84765625 , 0.60546875 ) ,
float2 ( 0.85156250 , 0.35546875 ) ,
float2 ( 0.85546875 , 0.85546875 ) ,
float2 ( 0.85937500 , 0.23046875 ) ,
float2 ( 0.86328125 , 0.73046875 ) ,
float2 ( 0.86718750 , 0.48046875 ) ,
float2 ( 0.87109375 , 0.98046875 ) ,
float2 ( 0.87500000 , 0.02734375 ) ,
float2 ( 0.87890625 , 0.52734375 ) ,
float2 ( 0.88281250 , 0.27734375 ) ,
float2 ( 0.88671875 , 0.77734375 ) ,
float2 ( 0.89062500 , 0.15234375 ) ,
float2 ( 0.89453125 , 0.65234375 ) ,
float2 ( 0.89843750 , 0.40234375 ) ,
float2 ( 0.90234375 , 0.90234375 ) ,
float2 ( 0.90625000 , 0.08984375 ) ,
float2 ( 0.91015625 , 0.58984375 ) ,
float2 ( 0.91406250 , 0.33984375 ) ,
float2 ( 0.91796875 , 0.83984375 ) ,
float2 ( 0.92187500 , 0.21484375 ) ,
float2 ( 0.92578125 , 0.71484375 ) ,
float2 ( 0.92968750 , 0.46484375 ) ,
float2 ( 0.93359375 , 0.96484375 ) ,
float2 ( 0.93750000 , 0.05859375 ) ,
float2 ( 0.94140625 , 0.55859375 ) ,
float2 ( 0.94531250 , 0.30859375 ) ,
float2 ( 0.94921875 , 0.80859375 ) ,
float2 ( 0.95312500 , 0.18359375 ) ,
float2 ( 0.95703125 , 0.68359375 ) ,
float2 ( 0.96093750 , 0.43359375 ) ,
float2 ( 0.96484375 , 0.93359375 ) ,
float2 ( 0.96875000 , 0.12109375 ) ,
float2 ( 0.97265625 , 0.62109375 ) ,
float2 ( 0.97656250 , 0.37109375 ) ,
float2 ( 0.98046875 , 0.87109375 ) ,
float2 ( 0.98437500 , 0.24609375 ) ,
float2 ( 0.98828125 , 0.74609375 ) ,
float2 ( 0.99218750 , 0.49609375 ) ,
float2 ( 0.99609375 , 0.99609375 )
} ;
//#line 421
float2 Hammersley2d ( uint i , uint sampleCount )
{
switch ( sampleCount )
{
//#line 431
case 16 : return k_Hammersley2dSeq16 [ i ] ;
case 32 : return k_Hammersley2dSeq32 [ i ] ;
case 64 : return k_Hammersley2dSeq64 [ i ] ;
case 256 : return k_Hammersley2dSeq256 [ i ] ;

default : return Hammersley2dSeq ( i , sampleCount ) ;
}
}
//#line 20 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/Sampling/Sampling.hlsl"
float3 SphericalToCartesian ( float cosPhi , float sinPhi , float cosTheta )
{
float sinTheta = SinFromCos ( cosTheta ) ;

return float3 ( float2 ( cosPhi , sinPhi ) * sinTheta , cosTheta ) ;
}

float3 SphericalToCartesian ( float phi , float cosTheta )
{
float sinPhi , cosPhi ;
sincos ( phi , sinPhi , cosPhi ) ;

return SphericalToCartesian ( cosPhi , sinPhi , cosTheta ) ;
}
//#line 38
float3 TransformGLtoDX ( float3 v )
{
return v . xzy ;
}
//#line 44
float3 ConvertEquiarealToCubemap ( float u , float v )
{
float phi = 6.28318530717958647693 - 6.28318530717958647693 * u ;
float cosTheta = 1.0 - 2.0 * v ;

return TransformGLtoDX ( SphericalToCartesian ( phi , cosTheta ) ) ;
}
//#line 53
float2 CubemapTexelToNVC ( uint2 unPositionTXS , uint cubemapSize )
{
return 2.0 * float2 ( unPositionTXS ) / float ( max ( cubemapSize - 1 , 1 ) ) - 1.0 ;
}
//#line 59
static const float3 CUBEMAP_FACE_BASIS_MAPPING [ 6 ] [ 3 ] =
{

{
float3 ( 0.0 , 0.0 , - 1.0 ) ,
float3 ( 0.0 , - 1.0 , 0.0 ) ,
float3 ( 1.0 , 0.0 , 0.0 )
} ,

{
float3 ( 0.0 , 0.0 , 1.0 ) ,
float3 ( 0.0 , - 1.0 , 0.0 ) ,
float3 ( - 1.0 , 0.0 , 0.0 )
} ,

{
float3 ( 1.0 , 0.0 , 0.0 ) ,
float3 ( 0.0 , 0.0 , 1.0 ) ,
float3 ( 0.0 , 1.0 , 0.0 )
} ,

{
float3 ( 1.0 , 0.0 , 0.0 ) ,
float3 ( 0.0 , 0.0 , - 1.0 ) ,
float3 ( 0.0 , - 1.0 , 0.0 )
} ,

{
float3 ( 1.0 , 0.0 , 0.0 ) ,
float3 ( 0.0 , - 1.0 , 0.0 ) ,
float3 ( 0.0 , 0.0 , 1.0 )
} ,

{
float3 ( - 1.0 , 0.0 , 0.0 ) ,
float3 ( 0.0 , - 1.0 , 0.0 ) ,
float3 ( 0.0 , 0.0 , - 1.0 )
}
} ;
//#line 100
float3 CubemapTexelToDirection ( float2 positionNVC , uint faceId )
{
float3 dir = CUBEMAP_FACE_BASIS_MAPPING [ faceId ] [ 0 ] * positionNVC . x
+ CUBEMAP_FACE_BASIS_MAPPING [ faceId ] [ 1 ] * positionNVC . y
+ CUBEMAP_FACE_BASIS_MAPPING [ faceId ] [ 2 ] ;

return normalize ( dir ) ;
}
//#line 116
float2 SampleDiskUniform ( float u1 , float u2 )
{
float r = sqrt ( u1 ) ;
float phi = 6.28318530717958647693 * u2 ;

float sinPhi , cosPhi ;
sincos ( phi , sinPhi , cosPhi ) ;

return r * float2 ( cosPhi , sinPhi ) ;
}
//#line 128
float2 SampleDiskCubic ( float u1 , float u2 )
{
float r = u1 ;
float phi = 6.28318530717958647693 * u2 ;

float sinPhi , cosPhi ;
sincos ( phi , sinPhi , cosPhi ) ;

return r * float2 ( cosPhi , sinPhi ) ;
}

float3 SampleConeUniform ( float u1 , float u2 , float cos_theta )
{
float r0 = cos_theta + u1 * ( 1.0f - cos_theta ) ;
float r = sqrt ( max ( 0.0 , 1.0 - r0 * r0 ) ) ;
float phi = 6.28318530717958647693 * u2 ;
return float3 ( r * cos ( phi ) , r * sin ( phi ) , r0 ) ;
}

float3 SampleSphereUniform ( float u1 , float u2 )
{
float phi = 6.28318530717958647693 * u2 ;
float cosTheta = 1.0 - 2.0 * u1 ;

return SphericalToCartesian ( phi , cosTheta ) ;
}
//#line 157
float3 SampleHemisphereCosine ( float u1 , float u2 )
{
float3 localL ;
//#line 163
localL . xy = SampleDiskUniform ( u1 , u2 ) ;
//#line 166
localL . z = sqrt ( 1.0 - u1 ) ;

return localL ;
}
//#line 173
float3 SampleHemisphereCosine ( float u1 , float u2 , float3 normal )
{
//#line 178
float3 pointOnSphere = SampleSphereUniform ( u1 , u2 ) ;
return SafeNormalize ( normal + pointOnSphere ) ;
}

float3 SampleHemisphereUniform ( float u1 , float u2 )
{
float phi = 6.28318530717958647693 * u2 ;
float cosTheta = 1.0 - u1 ;

return SphericalToCartesian ( phi , cosTheta ) ;
}

void SampleSphere ( float2 u ,
float4x4 localToWorld ,
float radius ,
out float lightPdf ,
out float3 P ,
out float3 Ns )
{
float u1 = u . x ;
float u2 = u . y ;

Ns = SampleSphereUniform ( u1 , u2 ) ;
//#line 203
P = radius * Ns + localToWorld [ 3 ] . xyz ;
//#line 206
lightPdf = 1.0 / ( 12.5663706143591729538 * radius * radius ) ;
}

void SampleHemisphere ( float2 u ,
float4x4 localToWorld ,
float radius ,
out float lightPdf ,
out float3 P ,
out float3 Ns )
{
float u1 = u . x ;
float u2 = u . y ;
//#line 220
Ns = - SampleHemisphereUniform ( u1 , u2 ) ;
P = radius * Ns ;
//#line 224
P = mul ( float4 ( P , 1.0 ) , localToWorld ) . xyz ;
Ns = mul ( Ns , ( float3x3 ) ( localToWorld ) ) ;
//#line 228
lightPdf = 1.0 / ( 6.28318530717958647693 * radius * radius ) ;
}
//#line 232
void SampleCylinder ( float2 u ,
float4x4 localToWorld ,
float radius ,
float width ,
out float lightPdf ,
out float3 P ,
out float3 Ns )
{
float u1 = u . x ;
float u2 = u . y ;
//#line 244
float t = ( u1 - 0.5 ) * width ;
float theta = 2.0 * 3.14159265358979323846 * u2 ;
float cosTheta = cos ( theta ) ;
float sinTheta = sin ( theta ) ;
//#line 250
P = float3 ( t , radius * cosTheta , radius * sinTheta ) ;
Ns = normalize ( float3 ( 0.0 , cosTheta , sinTheta ) ) ;
//#line 254
P = mul ( float4 ( P , 1.0 ) , localToWorld ) . xyz ;
Ns = mul ( Ns , ( float3x3 ) ( localToWorld ) ) ;
//#line 258
lightPdf = 1.0 / ( 6.28318530717958647693 * radius * width ) ;
}

void SampleRectangle ( float2 u ,
float4x4 localToWorld ,
float width ,
float height ,
out float lightPdf ,
out float3 P ,
out float3 Ns )
{

P = float3 ( ( u . x - 0.5 ) * width , ( u . y - 0.5 ) * height , 0 ) ;
Ns = float3 ( 0 , 0 , - 1 ) ;
//#line 274
P = mul ( float4 ( P , 1.0 ) , localToWorld ) . xyz ;
Ns = mul ( Ns , ( float3x3 ) ( localToWorld ) ) ;
//#line 278
lightPdf = 1.0 / ( width * height ) ;
}

void SampleDisk ( float2 u ,
float4x4 localToWorld ,
float radius ,
out float lightPdf ,
out float3 P ,
out float3 Ns )
{

P = float3 ( radius * SampleDiskUniform ( u . x , u . y ) , 0 ) ;
Ns = float3 ( 0.0 , 0.0 , - 1.0 ) ;
//#line 293
P = mul ( float4 ( P , 1.0 ) , localToWorld ) . xyz ;
Ns = mul ( Ns , ( float3x3 ) ( localToWorld ) ) ;
//#line 297
lightPdf = 1.0 / ( 3.14159265358979323846 * radius * radius ) ;
}
//#line 302
void SampleCone ( float2 u , float cosHalfAngle ,
out float3 dir , out float rcpPdf )
{
float cosTheta = lerp ( 1 , cosHalfAngle , u . x ) ;
float phi = 6.28318530717958647693 * u . y ;

dir = SphericalToCartesian ( phi , cosTheta ) ;
rcpPdf = 6.28318530717958647693 * ( 1 - cosHalfAngle ) ;
}
//#line 315
float3 SampleConeStrata ( uint sampleIdx , float rcpSampleCount , float cosHalfApexAngle )
{
float z = 1.0f - ( ( 1.0f - cosHalfApexAngle ) * sampleIdx ) * rcpSampleCount ;
float r = sqrt ( 1.0f - z * z ) ;
float a = sampleIdx * 2.3999632297286f ;
float sphi = sin ( a ) ;
float cphi = cos ( a ) ;
return float3 ( r * cphi , r * sphi , z ) ;
}
//#line 27 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
float PerceptualRoughnessToMipmapLevel ( float perceptualRoughness , uint maxMipLevel )
{
perceptualRoughness = perceptualRoughness * ( 1.7 - 0.7 * perceptualRoughness ) ;

return perceptualRoughness * maxMipLevel ;
}

float PerceptualRoughnessToMipmapLevel ( float perceptualRoughness )
{
return PerceptualRoughnessToMipmapLevel ( perceptualRoughness , 6 ) ;
}
//#line 44
float PerceptualRoughnessToMipmapLevel ( float perceptualRoughness , float NdotR )
{
float m = PerceptualRoughnessToRoughness ( perceptualRoughness ) ;
//#line 49
float n = ( 2.0 / max ( 5.960464478e-8 , m * m ) ) - 2.0 ;
//#line 52
n /= ( 4.0 * max ( NdotR , 5.960464478e-8 ) ) ;
//#line 55
perceptualRoughness = pow ( 2.0 / ( n + 2.0 ) , 0.25 ) ;

return perceptualRoughness * 6 ;
}
//#line 61
float MipmapLevelToPerceptualRoughness ( float mipmapLevel )
{
float perceptualRoughness = saturate ( mipmapLevel / 6 ) ;

return saturate ( 1.7 / 1.4 - sqrt ( 2.89 / 1.96 - ( 2.8 / 1.96 ) * perceptualRoughness ) ) ;
}
//#line 73
float3 ComputeViewFacingNormal ( float3 V , float3 T )
{
return Orthonormalize ( V , T ) ;
}
//#line 81
float3 GetAnisotropicModifiedNormal ( float3 grainDir , float3 N , float3 V , float anisotropy )
{
float3 grainNormal = ComputeViewFacingNormal ( V , grainDir ) ;
return normalize ( lerp ( N , grainNormal , anisotropy ) ) ;
}
//#line 92
void GetGGXAnisotropicModifiedNormalAndRoughness ( float3 bitangentWS , float3 tangentWS , float3 N , float3 V , float anisotropy , float perceptualRoughness , out float3 iblN , out float iblPerceptualRoughness )
{

float3 grainDirWS = ( anisotropy >= 0.0 ) ? bitangentWS : tangentWS ;

float stretch = abs ( anisotropy ) * saturate ( 1.5 * sqrt ( perceptualRoughness ) ) ;
//#line 100
iblN = GetAnisotropicModifiedNormal ( grainDirWS , N , V , stretch ) ;
iblPerceptualRoughness = perceptualRoughness * saturate ( 1.2 - abs ( anisotropy ) ) ;
}
//#line 105
float3 GetSpecularDominantDir ( float3 N , float3 R , float perceptualRoughness , float NdotV )
{
float p = perceptualRoughness ;
float a = 1.0 - p * p ;
float s = sqrt ( a ) ;
//#line 116
float lerpFactor = ( s + p * p ) * saturate ( a * a + lerp ( 0.0 , a , NdotV * NdotV ) ) ;
//#line 120
return lerp ( N , R , lerpFactor ) ;
}
//#line 127
void SampleGGXDir ( float2 u ,
float3 V ,
float3x3 localToWorld ,
float roughness ,
out float3 L ,
out float NdotL ,
out float NdotH ,
out float VdotH ,
bool VeqN = false )
{

float cosTheta = sqrt ( SafeDiv ( 1.0 - u . x , 1.0 + ( roughness * roughness - 1.0 ) * u . x ) ) ;
float phi = 6.28318530717958647693 * u . y ;

float3 localH = SphericalToCartesian ( phi , cosTheta ) ;

NdotH = cosTheta ;

float3 localV ;

if ( VeqN )
{

localV = float3 ( 0.0 , 0.0 , 1.0 ) ;
VdotH = NdotH ;
}
else
{
localV = mul ( V , transpose ( localToWorld ) ) ;
VdotH = saturate ( dot ( localV , localH ) ) ;
}
//#line 160
float3 localL = - localV + 2.0 * VdotH * localH ;
NdotL = localL . z ;

L = mul ( localL , localToWorld ) ;
}
//#line 167
void SampleAnisoGGXDir ( float2 u ,
float3 V ,
float3 N ,
float3 tangentX ,
float3 tangentY ,
float roughnessT ,
float roughnessB ,
out float3 H ,
out float3 L )
{

H = sqrt ( u . x / ( 1.0 - u . x ) ) * ( roughnessT * cos ( 6.28318530717958647693 * u . y ) * tangentX + roughnessB * sin ( 6.28318530717958647693 * u . y ) * tangentY ) + N ;
H = normalize ( H ) ;
//#line 182
L = 2.0 * saturate ( dot ( V , H ) ) * H - V ;
}
//#line 187
void SampleAnisoGGXVisibleNormal ( float2 u ,
float3 V ,
float3x3 localToWorld ,
float roughnessX ,
float roughnessY ,
out float3 localV ,
out float3 localH ,
out float VdotH )
{
localV = mul ( V , transpose ( localToWorld ) ) ;
//#line 199
float3x3 viewToLocal ;
viewToLocal [ 2 ] = normalize ( float3 ( roughnessX * localV . x , roughnessY * localV . y , localV . z ) ) ;
viewToLocal [ 0 ] = ( viewToLocal [ 2 ] . z < 0.9999 ) ? normalize ( cross ( float3 ( 0 , 0 , 1 ) , viewToLocal [ 2 ] ) ) : float3 ( 1 , 0 , 0 ) ;
viewToLocal [ 1 ] = cross ( viewToLocal [ 2 ] , viewToLocal [ 0 ] ) ;
//#line 205
float r = sqrt ( u . x ) ;
float phi = 2.0 * 3.14159265358979323846 * u . y ;
float t1 = r * cos ( phi ) ;
float t2 = r * sin ( phi ) ;
float s = 0.5 * ( 1.0 + viewToLocal [ 2 ] . z ) ;
t2 = ( 1.0 - s ) * sqrt ( 1.0 - t1 * t1 ) + s * t2 ;
//#line 213
localH = t1 * viewToLocal [ 0 ] + t2 * viewToLocal [ 1 ] + sqrt ( max ( 0.0 , 1.0 - t1 * t1 - t2 * t2 ) ) * viewToLocal [ 2 ] ;
//#line 216
localH = normalize ( float3 ( roughnessX * localH . x , roughnessY * localH . y , max ( 0.0 , localH . z ) ) ) ;

VdotH = saturate ( dot ( localV , localH ) ) ;
}
//#line 222
void SampleGGXVisibleNormal ( float2 u ,
float3 V ,
float3x3 localToWorld ,
float roughness ,
out float3 localV ,
out float3 localH ,
out float VdotH )
{
SampleAnisoGGXVisibleNormal ( u , V , localToWorld , roughness , roughness , localV , localH , VdotH ) ;
}
//#line 234
void ImportanceSampleLambert ( float2 u ,
float3x3 localToWorld ,
out float3 L ,
out float NdotL ,
out float weightOverPdf )
{
//#line 247
float3 N = localToWorld [ 2 ] ;

L = SampleHemisphereCosine ( u . x , u . y , N ) ;
NdotL = saturate ( dot ( N , L ) ) ;
//#line 261
weightOverPdf = 1.0 ;
}
//#line 265
void ImportanceSampleGGX ( float2 u ,
float3 V ,
float3x3 localToWorld ,
float roughness ,
float NdotV ,
out float3 L ,
out float VdotH ,
out float NdotL ,
out float weightOverPdf )
{
float NdotH ;
SampleGGXDir ( u , V , localToWorld , roughness , L , NdotL , NdotH , VdotH ) ;
//#line 287
float Vis = V_SmithJointGGX ( NdotL , NdotV , roughness ) ;
weightOverPdf = 4.0 * Vis * NdotL * VdotH / NdotH ;
}
//#line 292
void ImportanceSampleAnisoGGX ( float2 u ,
float3 V ,
float3x3 localToWorld ,
float roughnessT ,
float roughnessB ,
float NdotV ,
out float3 L ,
out float VdotH ,
out float NdotL ,
out float weightOverPdf )
{
float3 tangentX = localToWorld [ 0 ] ;
float3 tangentY = localToWorld [ 1 ] ;
float3 N = localToWorld [ 2 ] ;

float3 H ;
SampleAnisoGGXDir ( u , V , N , tangentX , tangentY , roughnessT , roughnessB , H , L ) ;

float NdotH = saturate ( dot ( N , H ) ) ;

VdotH = saturate ( dot ( V , H ) ) ;
NdotL = saturate ( dot ( N , L ) ) ;
//#line 325
float TdotV = dot ( tangentX , V ) ;
float BdotV = dot ( tangentY , V ) ;
float TdotL = dot ( tangentX , L ) ;
float BdotL = dot ( tangentY , L ) ;

float Vis = V_SmithJointGGXAniso ( TdotV , BdotV , NdotV , TdotL , BdotL , NdotL , roughnessT , roughnessB ) ;
weightOverPdf = 4.0 * Vis * NdotL * VdotH / NdotH ;
}
//#line 339
float4 IntegrateGGXAndDisneyDiffuseFGD ( float NdotV , float roughness , uint sampleCount = 4096 )
{
//#line 345
NdotV = max ( NdotV , 5.960464478e-8 ) ;
float3 V = float3 ( sqrt ( 1 - NdotV * NdotV ) , 0 , NdotV ) ;
float4 acc = float4 ( 0.0 , 0.0 , 0.0 , 0.0 ) ;

float3x3 localToWorld = k_identity3x3 ;

for ( uint i = 0 ; i < sampleCount ; ++ i )
{
float2 u = Hammersley2d ( i , sampleCount ) ;

float VdotH ;
float NdotL ;
float weightOverPdf ;

float3 L ;
ImportanceSampleGGX ( u , V , localToWorld , roughness , NdotV ,
L , VdotH , NdotL , weightOverPdf ) ;

if ( NdotL > 0.0 )
{
//#line 370
acc . x += weightOverPdf * pow ( 1 - VdotH , 5 ) ;
acc . y += weightOverPdf ;
}
//#line 375
ImportanceSampleLambert ( u , localToWorld , L , NdotL , weightOverPdf ) ;

if ( NdotL > 0.0 )
{
float LdotV = dot ( L , V ) ;
float disneyDiffuse = DisneyDiffuseNoPI ( NdotV , NdotL , LdotV , RoughnessToPerceptualRoughness ( roughness ) ) ;

acc . z += disneyDiffuse * weightOverPdf ;
}
}

acc /= sampleCount ;
//#line 389
acc . z -= 0.5 ;

return acc ;
}

uint GetIBLRuntimeFilterSampleCount ( uint mipLevel )
{
uint sampleCount = 0 ;

switch ( mipLevel )
{
case 1 : sampleCount = 21 ; break ;
case 2 : sampleCount = 34 ; break ;
//#line 408
case 3 : sampleCount = 55 ; break ;
case 4 : sampleCount = 89 ; break ;
case 5 : sampleCount = 89 ; break ;
case 6 : sampleCount = 89 ; break ;

}

return sampleCount ;
}
//#line 419
float4 IntegrateLD ( TextureCube tex , SamplerState sampl ,
Texture2D ggxIblSamples ,
float3 V ,
float3 N ,
float roughness ,
float index ,
float invOmegaP ,
uint sampleCount ,
bool prefilter ,
bool usePrecomputedSamples )
{
float3x3 localToWorld = GetLocalFrame ( N ) ;
//#line 433
float NdotV = 1 ;
float partLambdaV = GetSmithJointGGXPartLambdaV ( NdotV , roughness ) ;
//#line 437
float3 lightInt = float3 ( 0.0 , 0.0 , 0.0 ) ;
float cbsdfInt = 0.0 ;

for ( uint i = 0 ; i < sampleCount ; ++ i )
{
float3 L ;
float NdotL , NdotH , LdotH ;

if ( usePrecomputedSamples )
{
//#line 452
float3 localL = ggxIblSamples . Load ( int3 ( uint2 ( i , index ) , 0 ) ) . xyz ;

L = mul ( localL , localToWorld ) ;
NdotL = localL . z ;
LdotH = sqrt ( 0.5 + 0.5 * NdotL ) ;
}
else
{
float2 u = Fibonacci2d ( i , sampleCount ) ;
//#line 463
SampleGGXDir ( u , V , localToWorld , roughness , L , NdotL , NdotH , LdotH , true ) ;

if ( NdotL <= 0 ) continue ;
}

float mipLevel ;

if ( ! prefilter )
{
mipLevel = 0 ;
}
else
{
//#line 483
float omegaS ;

if ( usePrecomputedSamples )
{
omegaS = ggxIblSamples . Load ( int3 ( uint2 ( i , index ) , 0 ) ) . w ;
}
else
{
//#line 493
float pdf = 0.25 * D_GGX ( NdotH , roughness ) ;

omegaS = rcp ( sampleCount ) * rcp ( pdf ) ;
}
//#line 500
const float mipBias = roughness ;
mipLevel = 0.5 * log2 ( omegaS * invOmegaP ) + mipBias ;
}
//#line 505
float3 val = tex . SampleLevel ( sampl , L , mipLevel ) . rgb ;
//#line 520
float F = 1 ;
float G = V_SmithJointGGX ( NdotL , NdotV , roughness , partLambdaV ) * NdotL * NdotV ;

lightInt += F * G * val ;
cbsdfInt += F * G ;
//#line 530
}

return float4 ( lightInt / cbsdfInt , 1.0 ) ;
}

float4 IntegrateLDCharlie ( TextureCube tex , SamplerState sampl ,
float3 N ,
float roughness ,
uint sampleCount ,
float invFaceCenterTexelSolidAngle )
{

roughness = max ( roughness , 0.001f ) ;
sampleCount = max ( 1 , sampleCount ) ;
//#line 546
float3x3 localToWorld = GetLocalFrame ( N ) ;
float3 totalLight = float3 ( 0.0 , 0.0 , 0.0 ) ;
float totalWeight = 0.0 ;
float rcpNumSamples = rcp ( sampleCount ) ;
float pdf = 1 / ( 2.0f * 3.14159265358979323846 ) ;
float lodBias = roughness ;
float lodBase = 0.5f * log2 ( ( rcpNumSamples * 1.0f / pdf ) * invFaceCenterTexelSolidAngle ) + lodBias ;
for ( uint i = 0 ; i < sampleCount ; ++ i )
{

float3 localL = SampleConeStrata ( i , rcpNumSamples , 0.0f ) ;
float NdotL = localL . z ;
float3 L = mul ( localL , localToWorld ) ;
//#line 561
float NdotV = 1.0 ;
float LdotV , NdotH , LdotH , invLenLV ;
GetBSDFAngle ( N , L , NdotL , NdotV , LdotV , NdotH , LdotH , invLenLV ) ;
float D = D_Charlie ( NdotH , roughness ) ;
//#line 572
float3 cubeCoord = L / max ( abs ( L . x ) , max ( abs ( L . y ) , abs ( L . z ) ) ) ;
float invDu2 = dot ( cubeCoord , cubeCoord ) ;
float lod = 0.5f * 0.5f * log2 ( invDu2 ) + lodBase ;
float3 val = tex . SampleLevel ( sampl , L , lod ) . rgb ;
//#line 578
float w = D * NdotL ;
totalLight += val * w ;
totalWeight += w ;
}

return float4 ( totalLight / totalWeight , 1.0 ) ;
}
//#line 588
uint BinarySearchRow ( uint j , float needle , Texture2D haystack , uint n )
{
uint i = n - 1 ;
float v = haystack . Load ( int3 ( uint2 ( i , j ) , 0 ) ) . r ;

if ( needle < v )
{
i = 0 ;

for ( uint b = 1U << firstbithigh ( n - 1 ) ; b != 0 ; b >>= 1 )
{
uint p = i | b ;
v = haystack . Load ( int3 ( uint2 ( p , j ) , 0 ) ) . r ;
if ( v <= needle ) { i = p ; }
}
}

return i ;
}

float4 IntegrateLD_MIS ( TextureCube envMap , SamplerState sampler_envMap ,
Texture2D marginalRowDensities ,
Texture2D conditionalDensities ,
float3 V ,
float3 N ,
float roughness ,
float invOmegaP ,
uint width ,
uint height ,
uint sampleCount ,
bool prefilter )
{
float3x3 localToWorld = GetLocalFrame ( N ) ;

float3 lightInt = float3 ( 0.0 , 0.0 , 0.0 ) ;
float cbsdfInt = 0.0 ;
//#line 633
float envMapInt2dStep = marginalRowDensities . Load ( int3 ( uint2 ( height , 0 ) , 0 ) ) . r ;

float envMapIntSphere = envMapInt2dStep * 0.07957747154594766788 ;
//#line 638
for ( uint i = 0 ; i < sampleCount ; i ++ )
{
float2 s = Hammersley2d ( i , sampleCount ) ;
//#line 643
uint y = BinarySearchRow ( 0 , s . x , marginalRowDensities , height - 1 ) ;
//#line 646
uint x = BinarySearchRow ( y , s . y , conditionalDensities , width - 1 ) ;
//#line 651
float u = saturate ( ( float ) x / width + 1.0 / width ) ;
float v = saturate ( ( float ) y / height + 1.0 / height ) ;
float3 L = ConvertEquiarealToCubemap ( u , v ) ;

float NdotL = saturate ( dot ( N , L ) ) ;

if ( NdotL > 0.0 )
{
float3 val = envMap . SampleLevel ( sampler_envMap , L , 0 ) . rgb ;
float pdf = ( val . r + val . g + val . b ) / envMapIntSphere ;

if ( pdf > 0.0 )
{

float NdotH = sqrt ( NdotL * 0.5 + 0.5 ) ;
//#line 677
float weight = D_GGX ( NdotH , roughness ) * NdotL / ( 4.0 * pdf ) ;

lightInt += weight * val ;
cbsdfInt += weight ;
}
}
}
//#line 686
cbsdfInt = max ( cbsdfInt , 5.960464478e-8 ) ;

return float4 ( lightInt / cbsdfInt , 1.0 ) ;
}
//#line 693
float InfluenceFadeNormalWeight ( float3 normal , float3 centerToPos )
{

return saturate ( ( - 1.0f / 0.4f ) * dot ( normal , centerToPos ) + ( 0.6f / 0.4f ) ) ;
}
//#line 8 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
Texture2D _ScreenSpaceOcclusionTexture ;
//#line 13
struct AmbientOcclusionFactor
{
half indirectAmbientOcclusion ;
half directAmbientOcclusion ;
} ;

half SampleAmbientOcclusion ( float2 normalizedScreenSpaceUV )
{
float2 uv = normalizedScreenSpaceUV ;
return half ( _ScreenSpaceOcclusionTexture . SampleBias ( sampler_LinearClamp , uv , _GlobalMipBias . x ) . x ) ;
}

AmbientOcclusionFactor GetScreenSpaceAmbientOcclusion ( float2 normalizedScreenSpaceUV )
{
AmbientOcclusionFactor aoFactor ;

float ssao = saturate ( SampleAmbientOcclusion ( normalizedScreenSpaceUV ) + ( 1.0 - _AmbientOcclusionParam . x ) ) ;
aoFactor . indirectAmbientOcclusion = ssao ;
aoFactor . directAmbientOcclusion = lerp ( half ( 1.0 ) , ssao , _AmbientOcclusionParam . w ) ;
//#line 52
return aoFactor ;
}

AmbientOcclusionFactor CreateAmbientOcclusionFactor ( float2 normalizedScreenSpaceUV , half occlusion )
{
AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion ( normalizedScreenSpaceUV ) ;

aoFactor . indirectAmbientOcclusion = min ( aoFactor . indirectAmbientOcclusion , occlusion ) ;
return aoFactor ;
}

AmbientOcclusionFactor CreateAmbientOcclusionFactor ( InputData inputData , SurfaceData surfaceData )
{
return CreateAmbientOcclusionFactor ( inputData . normalizedScreenSpaceUV , surfaceData . occlusion ) ;
}
//#line 14 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
float SampleShadow_GetTriangleTexelArea ( float triangleHeight )
{
return triangleHeight - 0.5 ;
}
//#line 26
void SampleShadow_GetTexelAreas_Tent_3x3 ( float offset , out float4 computedArea , out float4 computedAreaUncut )
{

float offset01SquaredHalved = ( offset + 0.5 ) * ( offset + 0.5 ) * 0.5 ;
computedAreaUncut . x = computedArea . x = offset01SquaredHalved - offset ;
computedAreaUncut . w = computedArea . w = offset01SquaredHalved ;
//#line 36
computedAreaUncut . y = SampleShadow_GetTriangleTexelArea ( 1.5 - offset ) ;
//#line 39
float clampedOffsetLeft = min ( offset , 0 ) ;
float areaOfSmallLeftTriangle = clampedOffsetLeft * clampedOffsetLeft ;
computedArea . y = computedAreaUncut . y - areaOfSmallLeftTriangle ;
//#line 44
computedAreaUncut . z = SampleShadow_GetTriangleTexelArea ( 1.5 + offset ) ;
float clampedOffsetRight = max ( offset , 0 ) ;
float areaOfSmallRightTriangle = clampedOffsetRight * clampedOffsetRight ;
computedArea . z = computedAreaUncut . z - areaOfSmallRightTriangle ;
}
//#line 52
void SampleShadow_GetTexelWeights_Tent_3x3 ( float offset , out float4 computedWeight )
{
float4 dummy ;
SampleShadow_GetTexelAreas_Tent_3x3 ( offset , computedWeight , dummy ) ;
computedWeight *= 0.44444 ;
}
//#line 64
void SampleShadow_GetTexelWeights_Tent_5x5 ( float offset , out float3 texelsWeightsA , out float3 texelsWeightsB )
{

float4 computedArea_From3texelTriangle ;
float4 computedAreaUncut_From3texelTriangle ;
SampleShadow_GetTexelAreas_Tent_3x3 ( offset , computedArea_From3texelTriangle , computedAreaUncut_From3texelTriangle ) ;
//#line 74
texelsWeightsA . x = 0.16 * ( computedArea_From3texelTriangle . x ) ;
texelsWeightsA . y = 0.16 * ( computedAreaUncut_From3texelTriangle . y ) ;
texelsWeightsA . z = 0.16 * ( computedArea_From3texelTriangle . y + 1 ) ;
texelsWeightsB . x = 0.16 * ( computedArea_From3texelTriangle . z + 1 ) ;
texelsWeightsB . y = 0.16 * ( computedAreaUncut_From3texelTriangle . z ) ;
texelsWeightsB . z = 0.16 * ( computedArea_From3texelTriangle . w ) ;
}
//#line 87
void SampleShadow_GetTexelWeights_Tent_7x7 ( float offset , out float4 texelsWeightsA , out float4 texelsWeightsB )
{

float4 computedArea_From3texelTriangle ;
float4 computedAreaUncut_From3texelTriangle ;
SampleShadow_GetTexelAreas_Tent_3x3 ( offset , computedArea_From3texelTriangle , computedAreaUncut_From3texelTriangle ) ;
//#line 97
texelsWeightsA . x = 0.081632 * ( computedArea_From3texelTriangle . x ) ;
texelsWeightsA . y = 0.081632 * ( computedAreaUncut_From3texelTriangle . y ) ;
texelsWeightsA . z = 0.081632 * ( computedAreaUncut_From3texelTriangle . y + 1 ) ;
texelsWeightsA . w = 0.081632 * ( computedArea_From3texelTriangle . y + 2 ) ;
texelsWeightsB . x = 0.081632 * ( computedArea_From3texelTriangle . z + 2 ) ;
texelsWeightsB . y = 0.081632 * ( computedAreaUncut_From3texelTriangle . z + 1 ) ;
texelsWeightsB . z = 0.081632 * ( computedAreaUncut_From3texelTriangle . z ) ;
texelsWeightsB . w = 0.081632 * ( computedArea_From3texelTriangle . w ) ;
}
//#line 108
void SampleShadow_ComputeSamples_Tent_3x3 ( float4 shadowMapTexture_TexelSize , float2 coord , out float fetchesWeights [ 4 ] , out float2 fetchesUV [ 4 ] )
{

float2 tentCenterInTexelSpace = coord . xy * shadowMapTexture_TexelSize . zw ;
float2 centerOfFetchesInTexelSpace = floor ( tentCenterInTexelSpace + 0.5 ) ;
float2 offsetFromTentCenterToCenterOfFetches = tentCenterInTexelSpace - centerOfFetchesInTexelSpace ;
//#line 116
float4 texelsWeightsU , texelsWeightsV ;
SampleShadow_GetTexelWeights_Tent_3x3 ( offsetFromTentCenterToCenterOfFetches . x , texelsWeightsU ) ;
SampleShadow_GetTexelWeights_Tent_3x3 ( offsetFromTentCenterToCenterOfFetches . y , texelsWeightsV ) ;
//#line 121
float2 fetchesWeightsU = texelsWeightsU . xz + texelsWeightsU . yw ;
float2 fetchesWeightsV = texelsWeightsV . xz + texelsWeightsV . yw ;
//#line 125
float2 fetchesOffsetsU = texelsWeightsU . yw / fetchesWeightsU . xy + float2 ( - 1.5 , 0.5 ) ;
float2 fetchesOffsetsV = texelsWeightsV . yw / fetchesWeightsV . xy + float2 ( - 1.5 , 0.5 ) ;
fetchesOffsetsU *= shadowMapTexture_TexelSize . xx ;
fetchesOffsetsV *= shadowMapTexture_TexelSize . yy ;

float2 bilinearFetchOrigin = centerOfFetchesInTexelSpace * shadowMapTexture_TexelSize . xy ;
fetchesUV [ 0 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . x , fetchesOffsetsV . x ) ;
fetchesUV [ 1 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . y , fetchesOffsetsV . x ) ;
fetchesUV [ 2 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . x , fetchesOffsetsV . y ) ;
fetchesUV [ 3 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . y , fetchesOffsetsV . y ) ;

fetchesWeights [ 0 ] = fetchesWeightsU . x * fetchesWeightsV . x ;
fetchesWeights [ 1 ] = fetchesWeightsU . y * fetchesWeightsV . x ;
fetchesWeights [ 2 ] = fetchesWeightsU . x * fetchesWeightsV . y ;
fetchesWeights [ 3 ] = fetchesWeightsU . y * fetchesWeightsV . y ;
}
//#line 143
void SampleShadow_ComputeSamples_Tent_5x5 ( float4 shadowMapTexture_TexelSize , float2 coord , out float fetchesWeights [ 9 ] , out float2 fetchesUV [ 9 ] )
{

float2 tentCenterInTexelSpace = coord . xy * shadowMapTexture_TexelSize . zw ;
float2 centerOfFetchesInTexelSpace = floor ( tentCenterInTexelSpace + 0.5 ) ;
float2 offsetFromTentCenterToCenterOfFetches = tentCenterInTexelSpace - centerOfFetchesInTexelSpace ;
//#line 151
float3 texelsWeightsU_A , texelsWeightsU_B ;
float3 texelsWeightsV_A , texelsWeightsV_B ;
SampleShadow_GetTexelWeights_Tent_5x5 ( offsetFromTentCenterToCenterOfFetches . x , texelsWeightsU_A , texelsWeightsU_B ) ;
SampleShadow_GetTexelWeights_Tent_5x5 ( offsetFromTentCenterToCenterOfFetches . y , texelsWeightsV_A , texelsWeightsV_B ) ;
//#line 157
float3 fetchesWeightsU = float3 ( texelsWeightsU_A . xz , texelsWeightsU_B . y ) + float3 ( texelsWeightsU_A . y , texelsWeightsU_B . xz ) ;
float3 fetchesWeightsV = float3 ( texelsWeightsV_A . xz , texelsWeightsV_B . y ) + float3 ( texelsWeightsV_A . y , texelsWeightsV_B . xz ) ;
//#line 161
float3 fetchesOffsetsU = float3 ( texelsWeightsU_A . y , texelsWeightsU_B . xz ) / fetchesWeightsU . xyz + float3 ( - 2.5 , - 0.5 , 1.5 ) ;
float3 fetchesOffsetsV = float3 ( texelsWeightsV_A . y , texelsWeightsV_B . xz ) / fetchesWeightsV . xyz + float3 ( - 2.5 , - 0.5 , 1.5 ) ;
fetchesOffsetsU *= shadowMapTexture_TexelSize . xxx ;
fetchesOffsetsV *= shadowMapTexture_TexelSize . yyy ;

float2 bilinearFetchOrigin = centerOfFetchesInTexelSpace * shadowMapTexture_TexelSize . xy ;
fetchesUV [ 0 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . x , fetchesOffsetsV . x ) ;
fetchesUV [ 1 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . y , fetchesOffsetsV . x ) ;
fetchesUV [ 2 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . z , fetchesOffsetsV . x ) ;
fetchesUV [ 3 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . x , fetchesOffsetsV . y ) ;
fetchesUV [ 4 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . y , fetchesOffsetsV . y ) ;
fetchesUV [ 5 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . z , fetchesOffsetsV . y ) ;
fetchesUV [ 6 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . x , fetchesOffsetsV . z ) ;
fetchesUV [ 7 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . y , fetchesOffsetsV . z ) ;
fetchesUV [ 8 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . z , fetchesOffsetsV . z ) ;

fetchesWeights [ 0 ] = fetchesWeightsU . x * fetchesWeightsV . x ;
fetchesWeights [ 1 ] = fetchesWeightsU . y * fetchesWeightsV . x ;
fetchesWeights [ 2 ] = fetchesWeightsU . z * fetchesWeightsV . x ;
fetchesWeights [ 3 ] = fetchesWeightsU . x * fetchesWeightsV . y ;
fetchesWeights [ 4 ] = fetchesWeightsU . y * fetchesWeightsV . y ;
fetchesWeights [ 5 ] = fetchesWeightsU . z * fetchesWeightsV . y ;
fetchesWeights [ 6 ] = fetchesWeightsU . x * fetchesWeightsV . z ;
fetchesWeights [ 7 ] = fetchesWeightsU . y * fetchesWeightsV . z ;
fetchesWeights [ 8 ] = fetchesWeightsU . z * fetchesWeightsV . z ;
}
//#line 189
void SampleShadow_ComputeSamples_Tent_7x7 ( float4 shadowMapTexture_TexelSize , float2 coord , out float fetchesWeights [ 16 ] , out float2 fetchesUV [ 16 ] )
{

float2 tentCenterInTexelSpace = coord . xy * shadowMapTexture_TexelSize . zw ;
float2 centerOfFetchesInTexelSpace = floor ( tentCenterInTexelSpace + 0.5 ) ;
float2 offsetFromTentCenterToCenterOfFetches = tentCenterInTexelSpace - centerOfFetchesInTexelSpace ;
//#line 197
float4 texelsWeightsU_A , texelsWeightsU_B ;
float4 texelsWeightsV_A , texelsWeightsV_B ;
SampleShadow_GetTexelWeights_Tent_7x7 ( offsetFromTentCenterToCenterOfFetches . x , texelsWeightsU_A , texelsWeightsU_B ) ;
SampleShadow_GetTexelWeights_Tent_7x7 ( offsetFromTentCenterToCenterOfFetches . y , texelsWeightsV_A , texelsWeightsV_B ) ;
//#line 203
float4 fetchesWeightsU = float4 ( texelsWeightsU_A . xz , texelsWeightsU_B . xz ) + float4 ( texelsWeightsU_A . yw , texelsWeightsU_B . yw ) ;
float4 fetchesWeightsV = float4 ( texelsWeightsV_A . xz , texelsWeightsV_B . xz ) + float4 ( texelsWeightsV_A . yw , texelsWeightsV_B . yw ) ;
//#line 207
float4 fetchesOffsetsU = float4 ( texelsWeightsU_A . yw , texelsWeightsU_B . yw ) / fetchesWeightsU . xyzw + float4 ( - 3.5 , - 1.5 , 0.5 , 2.5 ) ;
float4 fetchesOffsetsV = float4 ( texelsWeightsV_A . yw , texelsWeightsV_B . yw ) / fetchesWeightsV . xyzw + float4 ( - 3.5 , - 1.5 , 0.5 , 2.5 ) ;
fetchesOffsetsU *= shadowMapTexture_TexelSize . xxxx ;
fetchesOffsetsV *= shadowMapTexture_TexelSize . yyyy ;

float2 bilinearFetchOrigin = centerOfFetchesInTexelSpace * shadowMapTexture_TexelSize . xy ;
fetchesUV [ 0 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . x , fetchesOffsetsV . x ) ;
fetchesUV [ 1 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . y , fetchesOffsetsV . x ) ;
fetchesUV [ 2 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . z , fetchesOffsetsV . x ) ;
fetchesUV [ 3 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . w , fetchesOffsetsV . x ) ;
fetchesUV [ 4 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . x , fetchesOffsetsV . y ) ;
fetchesUV [ 5 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . y , fetchesOffsetsV . y ) ;
fetchesUV [ 6 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . z , fetchesOffsetsV . y ) ;
fetchesUV [ 7 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . w , fetchesOffsetsV . y ) ;
fetchesUV [ 8 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . x , fetchesOffsetsV . z ) ;
fetchesUV [ 9 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . y , fetchesOffsetsV . z ) ;
fetchesUV [ 10 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . z , fetchesOffsetsV . z ) ;
fetchesUV [ 11 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . w , fetchesOffsetsV . z ) ;
fetchesUV [ 12 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . x , fetchesOffsetsV . w ) ;
fetchesUV [ 13 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . y , fetchesOffsetsV . w ) ;
fetchesUV [ 14 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . z , fetchesOffsetsV . w ) ;
fetchesUV [ 15 ] = bilinearFetchOrigin + float2 ( fetchesOffsetsU . w , fetchesOffsetsV . w ) ;

fetchesWeights [ 0 ] = fetchesWeightsU . x * fetchesWeightsV . x ;
fetchesWeights [ 1 ] = fetchesWeightsU . y * fetchesWeightsV . x ;
fetchesWeights [ 2 ] = fetchesWeightsU . z * fetchesWeightsV . x ;
fetchesWeights [ 3 ] = fetchesWeightsU . w * fetchesWeightsV . x ;
fetchesWeights [ 4 ] = fetchesWeightsU . x * fetchesWeightsV . y ;
fetchesWeights [ 5 ] = fetchesWeightsU . y * fetchesWeightsV . y ;
fetchesWeights [ 6 ] = fetchesWeightsU . z * fetchesWeightsV . y ;
fetchesWeights [ 7 ] = fetchesWeightsU . w * fetchesWeightsV . y ;
fetchesWeights [ 8 ] = fetchesWeightsU . x * fetchesWeightsV . z ;
fetchesWeights [ 9 ] = fetchesWeightsU . y * fetchesWeightsV . z ;
fetchesWeights [ 10 ] = fetchesWeightsU . z * fetchesWeightsV . z ;
fetchesWeights [ 11 ] = fetchesWeightsU . w * fetchesWeightsV . z ;
fetchesWeights [ 12 ] = fetchesWeightsU . x * fetchesWeightsV . w ;
fetchesWeights [ 13 ] = fetchesWeightsU . y * fetchesWeightsV . w ;
fetchesWeights [ 14 ] = fetchesWeightsU . z * fetchesWeightsV . w ;
fetchesWeights [ 15 ] = fetchesWeightsU . w * fetchesWeightsV . w ;
}
//#line 56 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
Texture2D _ScreenSpaceShadowmapTexture ;

Texture2D _MainLightShadowmapTexture ;
Texture2D _AdditionalLightsShadowmapTexture ;
SamplerComparisonState sampler_LinearClampCompare ;
//#line 64
cbuffer LightShadows {
//#line 70
float4x4 _MainLightWorldToShadow [ 4 + 1 ] ;
float4 _CascadeShadowSplitSpheres0 ;
float4 _CascadeShadowSplitSpheres1 ;
float4 _CascadeShadowSplitSpheres2 ;
float4 _CascadeShadowSplitSpheres3 ;
float4 _CascadeShadowSplitSphereRadii ;

float4 _MainLightShadowOffset0 ;
float4 _MainLightShadowOffset1 ;
float4 _MainLightShadowParams ;
float4 _MainLightShadowmapSize ;

float4 _AdditionalShadowOffset0 ;
float4 _AdditionalShadowOffset1 ;
float4 _AdditionalShadowFadeParams ;
float4 _AdditionalShadowmapSize ;
//#line 93
float4 _AdditionalShadowParams [ ( 256 ) ] ;
float4x4 _AdditionalLightsWorldToShadow [ ( 256 ) ] ;
//#line 99
} ;
//#line 109
float4 _ShadowBias ;
//#line 119
struct ShadowSamplingData
{
half4 shadowOffset0 ;
half4 shadowOffset1 ;
float4 shadowmapSize ;
half softShadowQuality ;
} ;

ShadowSamplingData GetMainLightShadowSamplingData ( )
{
ShadowSamplingData shadowSamplingData ;
//#line 132
shadowSamplingData . shadowOffset0 = half4 ( _MainLightShadowOffset0 ) ;
shadowSamplingData . shadowOffset1 = half4 ( _MainLightShadowOffset1 ) ;
//#line 136
shadowSamplingData . shadowmapSize = _MainLightShadowmapSize ;
shadowSamplingData . softShadowQuality = half ( _MainLightShadowParams . y ) ;

return shadowSamplingData ;
}

ShadowSamplingData GetAdditionalLightShadowSamplingData ( int index )
{
ShadowSamplingData shadowSamplingData = ( ShadowSamplingData ) 0 ;
//#line 148
shadowSamplingData . shadowOffset0 = _AdditionalShadowOffset0 ;
shadowSamplingData . shadowOffset1 = _AdditionalShadowOffset1 ;
//#line 152
shadowSamplingData . shadowmapSize = _AdditionalShadowmapSize ;
shadowSamplingData . softShadowQuality = _AdditionalShadowParams [ index ] . y ;
//#line 156
return shadowSamplingData ;
}
//#line 162
half4 GetMainLightShadowParams ( )
{
return half4 ( _MainLightShadowParams ) ;
}
//#line 173
half4 GetAdditionalLightShadowParams ( int lightIndex )
{
//#line 179
return _AdditionalShadowParams [ lightIndex ] ;
//#line 185
}

half SampleScreenSpaceShadowmap ( float4 shadowCoord )
{
shadowCoord . xy /= max ( 0.00001 , shadowCoord . w ) ;
//#line 192
shadowCoord . xy = shadowCoord . xy ;
//#line 197
half attenuation = half ( _ScreenSpaceShadowmapTexture . SampleBias ( sampler_PointClamp , shadowCoord . xy , _GlobalMipBias . x ) . x ) ;
//#line 200
return attenuation ;
}

float SampleShadowmapFilteredLowQuality ( Texture2D ShadowMap , SamplerComparisonState sampler_ShadowMap , float4 shadowCoord , ShadowSamplingData samplingData )
{

float4 attenuation4 ;
attenuation4 . x = float ( ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( shadowCoord . xyz + float3 ( samplingData . shadowOffset0 . xy , 0 ) ) . xy , ( shadowCoord . xyz + float3 ( samplingData . shadowOffset0 . xy , 0 ) ) . z ) ) ;
attenuation4 . y = float ( ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( shadowCoord . xyz + float3 ( samplingData . shadowOffset0 . zw , 0 ) ) . xy , ( shadowCoord . xyz + float3 ( samplingData . shadowOffset0 . zw , 0 ) ) . z ) ) ;
attenuation4 . z = float ( ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( shadowCoord . xyz + float3 ( samplingData . shadowOffset1 . xy , 0 ) ) . xy , ( shadowCoord . xyz + float3 ( samplingData . shadowOffset1 . xy , 0 ) ) . z ) ) ;
attenuation4 . w = float ( ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( shadowCoord . xyz + float3 ( samplingData . shadowOffset1 . zw , 0 ) ) . xy , ( shadowCoord . xyz + float3 ( samplingData . shadowOffset1 . zw , 0 ) ) . z ) ) ;
return dot ( attenuation4 , float ( 0.25 ) ) ;
}

float SampleShadowmapFilteredMediumQuality ( Texture2D ShadowMap , SamplerComparisonState sampler_ShadowMap , float4 shadowCoord , ShadowSamplingData samplingData )
{
float fetchesWeights [ 9 ] ;
float2 fetchesUV [ 9 ] ;
SampleShadow_ComputeSamples_Tent_5x5 ( samplingData . shadowmapSize , shadowCoord . xy , fetchesWeights , fetchesUV ) ;

return fetchesWeights [ 0 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 0 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 0 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 1 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 1 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 1 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 2 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 2 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 2 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 3 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 3 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 3 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 4 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 4 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 4 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 5 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 5 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 5 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 6 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 6 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 6 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 7 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 7 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 7 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 8 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 8 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 8 ] . xy , shadowCoord . z ) ) . z ) ;
}

float SampleShadowmapFilteredHighQuality ( Texture2D ShadowMap , SamplerComparisonState sampler_ShadowMap , float4 shadowCoord , ShadowSamplingData samplingData )
{
float fetchesWeights [ 16 ] ;
float2 fetchesUV [ 16 ] ;
SampleShadow_ComputeSamples_Tent_7x7 ( samplingData . shadowmapSize , shadowCoord . xy , fetchesWeights , fetchesUV ) ;

return fetchesWeights [ 0 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 0 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 0 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 1 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 1 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 1 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 2 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 2 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 2 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 3 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 3 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 3 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 4 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 4 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 4 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 5 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 5 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 5 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 6 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 6 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 6 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 7 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 7 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 7 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 8 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 8 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 8 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 9 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 9 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 9 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 10 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 10 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 10 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 11 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 11 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 11 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 12 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 12 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 12 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 13 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 13 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 13 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 14 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 14 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 14 ] . xy , shadowCoord . z ) ) . z )
+ fetchesWeights [ 15 ] * ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( float3 ( fetchesUV [ 15 ] . xy , shadowCoord . z ) ) . xy , ( float3 ( fetchesUV [ 15 ] . xy , shadowCoord . z ) ) . z ) ;
}

float SampleShadowmapFiltered ( Texture2D ShadowMap , SamplerComparisonState sampler_ShadowMap , float4 shadowCoord , ShadowSamplingData samplingData )
{
float attenuation = float ( 1.0 ) ;

if ( samplingData . softShadowQuality == half ( 1.0 ) )
{
attenuation = SampleShadowmapFilteredLowQuality ( ShadowMap , sampler_ShadowMap , shadowCoord , samplingData ) ;
}
else if ( samplingData . softShadowQuality == half ( 2.0 ) )
{
attenuation = SampleShadowmapFilteredMediumQuality ( ShadowMap , sampler_ShadowMap , shadowCoord , samplingData ) ;
}
else
{
attenuation = SampleShadowmapFilteredHighQuality ( ShadowMap , sampler_ShadowMap , shadowCoord , samplingData ) ;
}

return attenuation ;
}

float SampleShadowmap ( Texture2D ShadowMap , SamplerComparisonState sampler_ShadowMap , float4 shadowCoord , ShadowSamplingData samplingData , half4 shadowParams , bool isPerspectiveProjection = true )
{

if ( isPerspectiveProjection )
shadowCoord . xyz /= shadowCoord . w ;

float attenuation ;
float shadowStrength = shadowParams . x ;
//#line 292
if ( shadowParams . y > half ( 0.0 ) )
{
attenuation = SampleShadowmapFiltered ( ShadowMap , sampler_ShadowMap , shadowCoord , samplingData ) ;
}
else
{
attenuation = float ( ShadowMap . SampleCmpLevelZero ( sampler_ShadowMap , ( shadowCoord . xyz ) . xy , ( shadowCoord . xyz ) . z ) ) ;
}
//#line 304
attenuation = LerpWhiteTo ( attenuation , shadowStrength ) ;
//#line 308
return shadowCoord . z <= 0.0 || shadowCoord . z >= 1.0 ? 1.0 : attenuation ;
}

half ComputeCascadeIndex ( float3 positionWS )
{
float3 fromCenter0 = positionWS - _CascadeShadowSplitSpheres0 . xyz ;
float3 fromCenter1 = positionWS - _CascadeShadowSplitSpheres1 . xyz ;
float3 fromCenter2 = positionWS - _CascadeShadowSplitSpheres2 . xyz ;
float3 fromCenter3 = positionWS - _CascadeShadowSplitSpheres3 . xyz ;
float4 distances2 = float4 ( dot ( fromCenter0 , fromCenter0 ) , dot ( fromCenter1 , fromCenter1 ) , dot ( fromCenter2 , fromCenter2 ) , dot ( fromCenter3 , fromCenter3 ) ) ;

half4 weights = half4 ( distances2 < _CascadeShadowSplitSphereRadii ) ;
weights . yzw = saturate ( weights . yzw - weights . xyz ) ;

return half ( 4.0 ) - dot ( weights , half4 ( 4 , 3 , 2 , 1 ) ) ;
}

float4 TransformWorldToShadowCoord ( float3 positionWS )
{

half cascadeIndex = ComputeCascadeIndex ( positionWS ) ;
//#line 333
float4 shadowCoord = mul ( _MainLightWorldToShadow [ cascadeIndex ] , float4 ( positionWS , 1.0 ) ) ;

return float4 ( shadowCoord . xyz , 0 ) ;
}

half MainLightRealtimeShadow ( float4 shadowCoord )
{
//#line 345
ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData ( ) ;
half4 shadowParams = GetMainLightShadowParams ( ) ;
return SampleShadowmap ( _MainLightShadowmapTexture , sampler_LinearClampCompare , shadowCoord , shadowSamplingData , shadowParams , false ) ;

}
//#line 353
half AdditionalLightRealtimeShadow ( int lightIndex , float3 positionWS , half3 lightDirection )
{

ShadowSamplingData shadowSamplingData = GetAdditionalLightShadowSamplingData ( lightIndex ) ;

half4 shadowParams = GetAdditionalLightShadowParams ( lightIndex ) ;

int shadowSliceIndex = shadowParams . w ;
if ( shadowSliceIndex < 0 )
return 1.0 ;

half isPointLight = shadowParams . z ;

[ branch ]
if ( isPointLight )
{

float cubemapFaceId = CubeMapFaceID ( - lightDirection ) ;
shadowSliceIndex += cubemapFaceId ;
}
//#line 377
float4 shadowCoord = mul ( _AdditionalLightsWorldToShadow [ shadowSliceIndex ] , float4 ( positionWS , 1.0 ) ) ;
//#line 380
return SampleShadowmap ( _AdditionalLightsShadowmapTexture , sampler_LinearClampCompare , shadowCoord , shadowSamplingData , shadowParams , true ) ;
//#line 384
}

half GetMainLightShadowFade ( float3 positionWS )
{
float3 camToPixel = positionWS - _WorldSpaceCameraPos ;
float distanceCamToPixel2 = dot ( camToPixel , camToPixel ) ;

float fade = saturate ( distanceCamToPixel2 * float ( _MainLightShadowParams . z ) + float ( _MainLightShadowParams . w ) ) ;
return half ( fade ) ;
}

half GetAdditionalLightShadowFade ( float3 positionWS )
{

float3 camToPixel = positionWS - _WorldSpaceCameraPos ;
float distanceCamToPixel2 = dot ( camToPixel , camToPixel ) ;

float fade = saturate ( distanceCamToPixel2 * float ( _AdditionalShadowFadeParams . x ) + float ( _AdditionalShadowFadeParams . y ) ) ;
return half ( fade ) ;
//#line 406
}

half MixRealtimeAndBakedShadows ( half realtimeShadow , half bakedShadow , half shadowFade )
{
//#line 413
return lerp ( realtimeShadow , bakedShadow , shadowFade ) ;

}

half BakedShadow ( half4 shadowMask , half4 occlusionProbeChannels )
{
//#line 423
half bakedShadow = half ( 1.0 ) + dot ( shadowMask - half ( 1.0 ) , occlusionProbeChannels ) ;
return bakedShadow ;
}

half MainLightShadow ( float4 shadowCoord , float3 positionWS , half4 shadowMask , half4 occlusionProbeChannels )
{
half realtimeShadow = MainLightRealtimeShadow ( shadowCoord ) ;
//#line 434
half bakedShadow = half ( 1.0 ) ;
//#line 438
half shadowFade = GetMainLightShadowFade ( positionWS ) ;
//#line 443
return MixRealtimeAndBakedShadows ( realtimeShadow , bakedShadow , shadowFade ) ;
}

half AdditionalLightShadow ( int lightIndex , float3 positionWS , half3 lightDirection , half4 shadowMask , half4 occlusionProbeChannels )
{
half realtimeShadow = AdditionalLightRealtimeShadow ( lightIndex , positionWS , lightDirection ) ;
//#line 453
half bakedShadow = half ( 1.0 ) ;
//#line 457
half shadowFade = GetAdditionalLightShadowFade ( positionWS ) ;
//#line 462
return MixRealtimeAndBakedShadows ( realtimeShadow , bakedShadow , shadowFade ) ;
}

float4 GetShadowCoord ( VertexPositionInputs vertexInput )
{
//#line 470
return TransformWorldToShadowCoord ( vertexInput . positionWS ) ;

}

float3 ApplyShadowBias ( float3 positionWS , float3 normalWS , float3 lightDirection )
{
float invNdotL = 1.0 - saturate ( dot ( lightDirection , normalWS ) ) ;
float scale = invNdotL * _ShadowBias . y ;
//#line 480
positionWS = lightDirection * _ShadowBias . xxx + positionWS ;
positionWS = normalWS * scale . xxx + positionWS ;
return positionWS ;
}
//#line 493
float GetShadowFade ( float3 positionWS )
{
float3 camToPixel = positionWS - _WorldSpaceCameraPos ;
float distanceCamToPixel2 = dot ( camToPixel , camToPixel ) ;

float fade = saturate ( distanceCamToPixel2 * float ( _MainLightShadowParams . z ) + float ( _MainLightShadowParams . w ) ) ;
return fade * fade ;
}
//#line 503
float ApplyShadowFade ( float shadowAttenuation , float3 positionWS )
{
float fade = GetShadowFade ( positionWS ) ;
return shadowAttenuation + ( 1 - shadowAttenuation ) * fade * fade ;
}
//#line 510
half GetMainLightShadowStrength ( )
{
return half ( _MainLightShadowParams . x ) ;
}
//#line 516
half GetAdditionalLightShadowStrenth ( int lightIndex )
{
//#line 522
return half ( _AdditionalShadowParams [ lightIndex ] . x ) ;
//#line 527
}
//#line 530
float SampleShadowmap ( float4 shadowCoord , Texture2D ShadowMap , SamplerComparisonState sampler_ShadowMap , ShadowSamplingData samplingData , half shadowStrength , bool isPerspectiveProjection = true )
{
half4 shadowParams = half4 ( shadowStrength , 1.0 , 0.0 , 0.0 ) ;
return SampleShadowmap ( ShadowMap , sampler_ShadowMap , shadowCoord , samplingData , shadowParams , isPerspectiveProjection ) ;
}
//#line 537
half AdditionalLightRealtimeShadow ( int lightIndex , float3 positionWS )
{
return AdditionalLightRealtimeShadow ( lightIndex , positionWS , half3 ( 1 , 0 , 0 ) ) ;
}
//#line 8 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/LightCookie/LightCookieInput.hlsl"
Texture2D _MainLightCookieTexture ;
Texture2D _AdditionalLightsCookieAtlasTexture ;
//#line 12
SamplerState sampler_MainLightCookieTexture ;
//#line 17
cbuffer LightCookies {

float4x4 _MainLightWorldToLight ;
float _AdditionalLightsCookieEnableBits [ ( ( 256 ) + 31 ) / 32 ] ;
float _MainLightCookieTextureFormat ;
float _AdditionalLightsCookieAtlasTextureFormat ;

float4x4 _AdditionalLightsWorldToLights [ ( 256 ) ] ;
float4 _AdditionalLightsCookieAtlasUVRects [ ( 256 ) ] ;
float _AdditionalLightsLightTypes [ ( 256 ) ] ;
//#line 29
} ;
//#line 40
float4x4 GetLightCookieWorldToLightMatrix ( int lightIndex )
{
//#line 45
return _AdditionalLightsWorldToLights [ lightIndex ] ;

}

float4 GetLightCookieAtlasUVRect ( int lightIndex )
{
//#line 54
return _AdditionalLightsCookieAtlasUVRects [ lightIndex ] ;

}

int GetLightCookieLightType ( int lightIndex )
{
//#line 63
return _AdditionalLightsLightTypes [ lightIndex ] ;

}

bool IsMainLightCookieTextureRGBFormat ( )
{
return _MainLightCookieTextureFormat == ( 0 ) ;
}

bool IsMainLightCookieTextureAlphaFormat ( )
{
return _MainLightCookieTextureFormat == ( 1 ) ;
}

bool IsAdditionalLightsCookieAtlasTextureRGBFormat ( )
{
return _AdditionalLightsCookieAtlasTextureFormat == ( 0 ) ;
}

bool IsAdditionalLightsCookieAtlasTextureAlphaFormat ( )
{
return _AdditionalLightsCookieAtlasTextureFormat == ( 1 ) ;
}
//#line 89
float4 SampleMainLightCookieTexture ( float2 uv )
{
return _MainLightCookieTexture . SampleBias ( sampler_MainLightCookieTexture , uv , _GlobalMipBias . x ) ;
}

float4 SampleAdditionalLightsCookieAtlasTexture ( float2 uv )
{

return _AdditionalLightsCookieAtlasTexture . SampleLevel ( sampler_LinearClamp , uv , 0 ) ;
}
//#line 101
bool IsMainLightCookieEnabled ( )
{
return _MainLightCookieTextureFormat != ( - 1 ) ;
}

bool IsLightCookieEnabled ( int lightBufferIndex )
{
//#line 113
uint elemIndex = ( ( uint ) lightBufferIndex ) >> 5 ;
uint bitOffset = ( uint ) lightBufferIndex & ( ( 1 << 5 ) - 1 ) ;
//#line 119
uint elem = asuint ( _AdditionalLightsCookieEnableBits [ elemIndex ] ) ;
//#line 122
return ( elem & ( 1u << bitOffset ) ) != 0u ;

}
//#line 16 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/LightCookie/LightCookie.hlsl"
float2 ComputeLightCookieUVDirectional ( float4x4 worldToLight , float3 samplePositionWS , float4 atlasUVRect , uint2 uvWrap )
{
//#line 20
float2 positionLS = mul ( worldToLight , float4 ( samplePositionWS , 1 ) ) . xy ;
//#line 24
float2 positionUV = positionLS * 0.5 + 0.5 ;
//#line 27
positionUV . x = ( uvWrap . x == 0 ) ? frac ( positionUV . x ) : positionUV . x ;
positionUV . y = ( uvWrap . y == 0 ) ? frac ( positionUV . y ) : positionUV . y ;
positionUV . x = ( uvWrap . x == 1 ) ? saturate ( positionUV . x ) : positionUV . x ;
positionUV . y = ( uvWrap . y == 1 ) ? saturate ( positionUV . y ) : positionUV . y ;
//#line 33
float2 positionAtlasUV = atlasUVRect . xy * float2 ( positionUV ) + atlasUVRect . zw ;

return positionAtlasUV ;
}

float2 ComputeLightCookieUVSpot ( float4x4 worldToLightPerspective , float3 samplePositionWS , float4 atlasUVRect )
{

float4 positionCS = mul ( worldToLightPerspective , float4 ( samplePositionWS , 1 ) ) ;
float2 positionNDC = positionCS . xy / positionCS . w ;
//#line 45
float2 positionUV = saturate ( positionNDC * 0.5 + 0.5 ) ;
//#line 48
float2 positionAtlasUV = atlasUVRect . xy * float2 ( positionUV ) + atlasUVRect . zw ;

return positionAtlasUV ;
}

float2 ComputeLightCookieUVPoint ( float4x4 worldToLight , float3 samplePositionWS , float4 atlasUVRect )
{

float4 positionLS = mul ( worldToLight , float4 ( samplePositionWS , 1 ) ) ;

float3 sampleDirLS = normalize ( positionLS . xyz / positionLS . w ) ;
//#line 61
float2 positionUV = saturate ( PackNormalOctQuadEncode ( sampleDirLS ) * 0.5 + 0.5 ) ;
//#line 64
float2 positionAtlasUV = atlasUVRect . xy * float2 ( positionUV ) + atlasUVRect . zw ;

return positionAtlasUV ;
}
//#line 71
float3 SampleMainLightCookie ( float3 samplePositionWS )
{
if ( ! IsMainLightCookieEnabled ( ) )
return float3 ( 1 , 1 , 1 ) ;

float2 uv = ComputeLightCookieUVDirectional ( _MainLightWorldToLight , samplePositionWS , float4 ( 1 , 1 , 0 , 0 ) , - 1 ) ;
float4 color = SampleMainLightCookieTexture ( uv ) ;

return IsMainLightCookieTextureRGBFormat ( ) ? color . rgb
: IsMainLightCookieTextureAlphaFormat ( ) ? color . aaa
: color . rrr ;
}

float3 SampleAdditionalLightCookie ( int perObjectLightIndex , float3 samplePositionWS )
{
if ( ! IsLightCookieEnabled ( perObjectLightIndex ) )
return float3 ( 1 , 1 , 1 ) ;

int lightType = GetLightCookieLightType ( perObjectLightIndex ) ;
int isSpot = lightType == 0 ;
int isDirectional = lightType == 1 ;

float4x4 worldToLight = GetLightCookieWorldToLightMatrix ( perObjectLightIndex ) ;
float4 uvRect = GetLightCookieAtlasUVRect ( perObjectLightIndex ) ;

float2 uv ;
if ( isSpot )
{
uv = ComputeLightCookieUVSpot ( worldToLight , samplePositionWS , uvRect ) ;
}
else if ( isDirectional )
{
uv = ComputeLightCookieUVDirectional ( worldToLight , samplePositionWS , uvRect , 0 ) ;
}
else
{
uv = ComputeLightCookieUVPoint ( worldToLight , samplePositionWS , uvRect ) ;
}

float4 color = SampleAdditionalLightsCookieAtlasTexture ( uv ) ;

return IsAdditionalLightsCookieAtlasTextureRGBFormat ( ) ? color . rgb
: IsAdditionalLightsCookieAtlasTextureAlphaFormat ( ) ? color . aaa
: color . rrr ;
}
//#line 107 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
float2 FoveatedRemapLinearToNonUniform ( float2 uv ) { return uv ; }
float2 FoveatedRemapPrevFrameLinearToNonUniform ( float2 uv ) { return uv ; }
float2 FoveatedRemapDensity ( float2 uv ) { return uv ; }
float2 FoveatedRemapPrevFrameDensity ( float2 uv ) { return uv ; }
float2 FoveatedRemapNonUniformToLinear ( float2 uv ) { return uv ; }
float2 FoveatedRemapPrevFrameNonUniformToLinear ( float2 uv ) { return uv ; }
int2 FoveatedRemapLinearToNonUniformCS ( int2 positionCS ) { return positionCS ; }
int2 FoveatedRemapNonUniformToLinearCS ( int2 positionCS ) { return positionCS ; }
//#line 119
PositionInputs FoveatedGetPositionInput ( float2 positionSS , float2 invScreenSize , uint2 tileCoord )
{
PositionInputs posInput = GetPositionInput ( positionSS , invScreenSize , tileCoord ) ;
posInput . positionNDC = FoveatedRemapNonUniformToLinear ( posInput . positionNDC ) ;
return posInput ;
}

PositionInputs FoveatedPrevFrameGetPositionInput ( float2 positionSS , float2 invScreenSize , uint2 tileCoord )
{
PositionInputs posInput = GetPositionInput ( positionSS , invScreenSize , tileCoord ) ;
posInput . positionNDC = FoveatedRemapPrevFrameNonUniformToLinear ( posInput . positionNDC ) ;
return posInput ;
}

PositionInputs FoveatedGetPositionInput ( float2 positionSS , float2 invScreenSize )
{
return FoveatedGetPositionInput ( positionSS , invScreenSize , uint2 ( 0 , 0 ) ) ;
}

PositionInputs FoveatedPrevFrameGetPositionInput ( float2 positionSS , float2 invScreenSize )
{
return FoveatedPrevFrameGetPositionInput ( positionSS , invScreenSize , uint2 ( 0 , 0 ) ) ;
}

PositionInputs FoveatedGetPositionInput ( float2 positionSS , float2 invScreenSize , float3 positionWS )
{
PositionInputs posInput = FoveatedGetPositionInput ( positionSS , invScreenSize , uint2 ( 0 , 0 ) ) ;
posInput . positionWS = positionWS ;
return posInput ;
}

PositionInputs FoveatedPrevFrameGetPositionInput ( float2 positionSS , float2 invScreenSize , float3 positionWS )
{
PositionInputs posInput = FoveatedPrevFrameGetPositionInput ( positionSS , invScreenSize , uint2 ( 0 , 0 ) ) ;
posInput . positionWS = positionWS ;
return posInput ;
}

PositionInputs FoveatedGetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth , float linearDepth , float3 positionWS , uint2 tileCoord )
{
PositionInputs posInput = FoveatedGetPositionInput ( positionSS , invScreenSize , tileCoord ) ;
posInput . positionWS = positionWS ;
posInput . deviceDepth = deviceDepth ;
posInput . linearDepth = linearDepth ;

return posInput ;
}

PositionInputs FoveatedPrevFrameGetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth , float linearDepth , float3 positionWS , uint2 tileCoord )
{
PositionInputs posInput = FoveatedPrevFrameGetPositionInput ( positionSS , invScreenSize , tileCoord ) ;
posInput . positionWS = positionWS ;
posInput . deviceDepth = deviceDepth ;
posInput . linearDepth = linearDepth ;

return posInput ;
}

PositionInputs FoveatedGetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth , float linearDepth , float3 positionWS )
{
return FoveatedGetPositionInput ( positionSS , invScreenSize , deviceDepth , linearDepth , positionWS , uint2 ( 0 , 0 ) ) ;
}

PositionInputs FoveatedPrevFrameGetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth , float linearDepth , float3 positionWS )
{
return FoveatedPrevFrameGetPositionInput ( positionSS , invScreenSize , deviceDepth , linearDepth , positionWS , uint2 ( 0 , 0 ) ) ;
}

PositionInputs FoveatedGetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth ,
float4x4 invViewProjMatrix , float4x4 viewMatrix ,
uint2 tileCoord )
{
PositionInputs posInput = FoveatedGetPositionInput ( positionSS , invScreenSize , tileCoord ) ;
posInput . positionWS = ComputeWorldSpacePosition ( posInput . positionNDC , deviceDepth , invViewProjMatrix ) ;
posInput . deviceDepth = deviceDepth ;
posInput . linearDepth = LinearEyeDepth ( posInput . positionWS , viewMatrix ) ;

return posInput ;
}

PositionInputs FoveatedPrevFrameGetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth ,
float4x4 invViewProjMatrix , float4x4 viewMatrix ,
uint2 tileCoord )
{
PositionInputs posInput = FoveatedPrevFrameGetPositionInput ( positionSS , invScreenSize , tileCoord ) ;
posInput . positionWS = ComputeWorldSpacePosition ( posInput . positionNDC , deviceDepth , invViewProjMatrix ) ;
posInput . deviceDepth = deviceDepth ;
posInput . linearDepth = LinearEyeDepth ( posInput . positionWS , viewMatrix ) ;

return posInput ;
}

PositionInputs FoveatedGetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth ,
float4x4 invViewProjMatrix , float4x4 viewMatrix )
{
return FoveatedGetPositionInput ( positionSS , invScreenSize , deviceDepth , invViewProjMatrix , viewMatrix , uint2 ( 0 , 0 ) ) ;
}

PositionInputs FoveatedPrevFrameGetPositionInput ( float2 positionSS , float2 invScreenSize , float deviceDepth ,
float4x4 invViewProjMatrix , float4x4 viewMatrix )
{
return FoveatedPrevFrameGetPositionInput ( positionSS , invScreenSize , deviceDepth , invViewProjMatrix , viewMatrix , uint2 ( 0 , 0 ) ) ;
}
//#line 15 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/Clustering.hlsl"
struct ClusterIterator
{
uint tileOffset ;
uint zBinOffset ;
uint tileMask ;

uint entityIndexNextMax ;
} ;
//#line 25
ClusterIterator ClusterInit ( float2 normalizedScreenSpaceUV , float3 positionWS , int headerIndex )
{
ClusterIterator state = ( ClusterIterator ) 0 ;
//#line 44
uint2 tileId = uint2 ( normalizedScreenSpaceUV * ( ( float2 ) _FPParams1 . xy ) ) ;
state . tileOffset = tileId . y * ( ( uint ) _FPParams1 . z ) + tileId . x ;
//#line 49
state . tileOffset *= ( ( uint ) _FPParams1 . w ) ;

float viewZ = dot ( GetViewForwardDir ( ) , positionWS - GetCameraPositionWS ( ) ) ;
uint zBinBaseIndex = ( uint ) ( ( IsPerspectiveProjection ( ) ? log2 ( viewZ ) : viewZ ) * ( _FPParams0 . x ) + ( _FPParams0 . y ) ) ;
//#line 66
zBinBaseIndex = zBinBaseIndex * ( 2 + ( ( uint ) _FPParams1 . w ) ) ;
zBinBaseIndex = min ( zBinBaseIndex , 4 * 1024 - ( 2 + ( ( uint ) _FPParams1 . w ) ) ) ;

uint zBinHeaderIndex = zBinBaseIndex + headerIndex ;
state . zBinOffset = zBinBaseIndex + 2 ;
//#line 73
uint header = Select4 ( asuint ( urp_ZBins [ zBinHeaderIndex / 4 ] ) , zBinHeaderIndex % 4 ) ;
//#line 78
state . entityIndexNextMax = header ;
//#line 91
return state ;
}
//#line 95
bool ClusterNext ( inout ClusterIterator it , out uint entityIndex )
{

uint maxIndex = it . entityIndexNextMax >> 16 ;
[ loop ] while ( it . tileMask == 0 && ( it . entityIndexNextMax & 0xFFFF ) <= maxIndex )
{

uint wordIndex = ( ( it . entityIndexNextMax & 0xFFFF ) >> 5 ) ;
uint tileIndex = it . tileOffset + wordIndex ;
uint zBinIndex = it . zBinOffset + wordIndex ;
it . tileMask =

Select4 ( asuint ( urp_Tiles [ tileIndex / 4 ] ) , tileIndex % 4 ) &
//#line 110
Select4 ( asuint ( urp_ZBins [ zBinIndex / 4 ] ) , zBinIndex % 4 ) &
//#line 113
( 0xFFFFFFFFu << ( it . entityIndexNextMax & 0x1F ) ) & ( 0xFFFFFFFFu >> ( 31 - min ( 31 , maxIndex - wordIndex * 32 ) ) ) ;
//#line 116
it . entityIndexNextMax = ( it . entityIndexNextMax + 32 ) & ~ 31 ;
}

bool hasNext = it . tileMask != 0 ;
uint bitIndex = firstbitlow ( it . tileMask ) ;
it . tileMask ^= ( 1 << bitIndex ) ;
//#line 126
entityIndex = ( ( ( it . entityIndexNextMax - 32 ) & ( 0xFFFF & ~ 31 ) ) ) + bitIndex ;
//#line 130
return hasNext ;
}
//#line 12 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
struct Light
{
half3 direction ;
half3 color ;
float distanceAttenuation ;
half shadowAttenuation ;
uint layerMask ;
} ;
//#line 47
float DistanceAttenuation ( float distanceSqr , half2 distanceAttenuation )
{
//#line 51
float lightAtten = rcp ( distanceSqr ) ;
float2 distanceAttenuationFloat = float2 ( distanceAttenuation ) ;
//#line 55
half factor = half ( distanceSqr * distanceAttenuationFloat . x ) ;
half smoothFactor = saturate ( half ( 1.0 ) - factor * factor ) ;
smoothFactor = smoothFactor * smoothFactor ;

return lightAtten * smoothFactor ;
}

half AngleAttenuation ( half3 spotDirection , half3 lightDirection , half2 spotAttenuation )
{
//#line 72
half SdotL = dot ( spotDirection , lightDirection ) ;
half atten = saturate ( SdotL * spotAttenuation . x + spotAttenuation . y ) ;
return atten * atten ;
}
//#line 81
Light GetMainLight ( )
{
Light light ;
light . direction = half3 ( _MainLightPosition . xyz ) ;
//#line 89
light . distanceAttenuation = 1.0 ;
//#line 94
light . shadowAttenuation = 1.0 ;
light . color = _MainLightColor . rgb ;

light . layerMask = _MainLightLayerMask ;

return light ;
}

Light GetMainLight ( float4 shadowCoord )
{
Light light = GetMainLight ( ) ;
light . shadowAttenuation = MainLightRealtimeShadow ( shadowCoord ) ;
return light ;
}

Light GetMainLight ( float4 shadowCoord , float3 positionWS , half4 shadowMask )
{
Light light = GetMainLight ( ) ;
light . shadowAttenuation = MainLightShadow ( shadowCoord , positionWS , shadowMask , _MainLightOcclusionProbes ) ;
//#line 119
return light ;
}

Light GetMainLight ( InputData inputData , half4 shadowMask , AmbientOcclusionFactor aoFactor )
{
Light light = GetMainLight ( inputData . shadowCoord , inputData . positionWS , shadowMask ) ;
//#line 127
if ( IsLightingFeatureEnabled ( ( 32 ) ) )
{
light . color *= aoFactor . directAmbientOcclusion ;
}
//#line 133
return light ;
}
//#line 137
Light GetAdditionalPerObjectLight ( int perObjectLightIndex , float3 positionWS )
{
//#line 147
float4 lightPositionWS = _AdditionalLightsPosition [ perObjectLightIndex ] ;
half3 color = _AdditionalLightsColor [ perObjectLightIndex ] . rgb ;
half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation [ perObjectLightIndex ] ;
half4 spotDirection = _AdditionalLightsSpotDir [ perObjectLightIndex ] ;
uint lightLayerMask = asuint ( _AdditionalLightsLayerMasks [ perObjectLightIndex ] ) ;
//#line 156
float3 lightVector = lightPositionWS . xyz - positionWS * lightPositionWS . w ;
float distanceSqr = max ( dot ( lightVector , lightVector ) , 6.103515625e-5 ) ;

half3 lightDirection = half3 ( lightVector * rsqrt ( distanceSqr ) ) ;

float attenuation = DistanceAttenuation ( distanceSqr , distanceAndSpotAttenuation . xy ) * AngleAttenuation ( spotDirection . xyz , lightDirection , distanceAndSpotAttenuation . zw ) ;

Light light ;
light . direction = lightDirection ;
light . distanceAttenuation = attenuation ;
light . shadowAttenuation = 1.0 ;
light . color = color ;
light . layerMask = lightLayerMask ;

return light ;
}

uint GetPerObjectLightIndexOffset ( )
{
//#line 178
return 0 ;

}
//#line 184
int GetPerObjectLightIndex ( uint index )
{
//#line 217
float4 tmp = unity_LightIndices [ index / 4 ] ;
return int ( tmp [ index % 4 ] ) ;

}
//#line 224
Light GetAdditionalLight ( uint i , float3 positionWS )
{

int lightIndex = i ;
//#line 231
return GetAdditionalPerObjectLight ( lightIndex , positionWS ) ;
}

Light GetAdditionalLight ( uint i , float3 positionWS , half4 shadowMask )
{

int lightIndex = i ;
//#line 241
Light light = GetAdditionalPerObjectLight ( lightIndex , positionWS ) ;
//#line 246
half4 occlusionProbeChannels = _AdditionalLightsOcclusionProbes [ lightIndex ] ;

light . shadowAttenuation = AdditionalLightShadow ( lightIndex , positionWS , light . direction , shadowMask , occlusionProbeChannels ) ;
//#line 254
return light ;
}

Light GetAdditionalLight ( uint i , InputData inputData , half4 shadowMask , AmbientOcclusionFactor aoFactor )
{
Light light = GetAdditionalLight ( i , inputData . positionWS , shadowMask ) ;
//#line 262
if ( IsLightingFeatureEnabled ( ( 32 ) ) )
{
light . color *= aoFactor . directAmbientOcclusion ;
}
//#line 268
return light ;
}

int GetAdditionalLightsCount ( )
{
//#line 275
return 0 ;
//#line 282
}

half4 CalculateShadowMask ( InputData inputData )
{
//#line 290
half4 shadowMask = unity_ProbesOcclusion ;
//#line 295
return shadowMask ;
}
//#line 9 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/ShaderLibrary/AmbientProbe.hlsl"
void ReconvolveAmbientProbeWithPower ( float diffusePower , inout float4 SHCoefficients [ 7 ] )
{
if ( diffusePower == 0.0f )
return ;
//#line 15
float w = diffusePower + 1 ;
float kModifiedLambertian0 = 1.0f ;
float kModifiedLambertian1 = ( w + 1.0f ) / ( w + 2.0f ) ;
float kModifiedLambertian2 = w / ( w + 3.0f ) ;
//#line 22
float wrapScaling0 = kModifiedLambertian0 / ( 1.0f ) ;
float wrapScaling1 = kModifiedLambertian1 / ( 2.0f / 3.0f ) ;
float wrapScaling2 = kModifiedLambertian2 / ( 1.0f / 4.0f ) ;
//#line 27
float3 ambient6 = float3 ( SHCoefficients [ 3 ] . z , SHCoefficients [ 4 ] . z , SHCoefficients [ 5 ] . z ) / 3.0f ;
float3 ambient0 = float3 ( SHCoefficients [ 0 ] . a , SHCoefficients [ 1 ] . a , SHCoefficients [ 2 ] . a ) + ambient6 ;

SHCoefficients [ 0 ] . xyz *= wrapScaling1 ;
SHCoefficients [ 1 ] . xyz *= wrapScaling1 ;
SHCoefficients [ 2 ] . xyz *= wrapScaling1 ;
SHCoefficients [ 3 ] *= wrapScaling2 ;
SHCoefficients [ 4 ] *= wrapScaling2 ;
SHCoefficients [ 5 ] *= wrapScaling2 ;
SHCoefficients [ 6 ] *= wrapScaling2 ;

SHCoefficients [ 0 ] . a = ambient0 . r * wrapScaling0 - ambient6 . r * wrapScaling2 ;
SHCoefficients [ 1 ] . a = ambient0 . g * wrapScaling0 - ambient6 . g * wrapScaling2 ;
SHCoefficients [ 2 ] . a = ambient0 . b * wrapScaling0 - ambient6 . b * wrapScaling2 ;
}
//#line 45
float3 EvaluateAmbientProbe ( float3 normalWS )
{
//#line 51
float3 res = SHEvalLinearL0L1 ( normalWS , unity_SHAr , unity_SHAg , unity_SHAb ) ;
//#line 54
res += SHEvalLinearL2 ( normalWS , unity_SHBr , unity_SHBg , unity_SHBb , unity_SHC ) ;

return res ;

}

float3 EvaluateAmbientProbeSRGB ( float3 normalWS )
{
float3 res = EvaluateAmbientProbe ( normalWS ) ;
//#line 66
return res ;
}

float3 SampleSH ( float3 normalWS )
{
return EvaluateAmbientProbeSRGB ( normalWS ) ;
}

float3 EvaluateAmbientProbeL1 ( float3 normalWS )
{
//#line 83
return float3 ( 0.0 , 0.0 , 0.0 ) ;

}

float3 EvaluateAmbientProbeL0 ( )
{
//#line 92
return float3 ( 0.0 , 0.0 , 0.0 ) ;

}
//#line 15 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/Runtime/Lighting/ProbeVolume/ShaderVariablesProbeVolumes.cs.hlsl"
cbuffer ShaderVariablesProbeVolumes {
float4 _Offset_IndirectionEntryDim ;
float4 _Weight_MinLoadedCellInEntries ;
float4 _PoolDim_MinBrickSize ;
float4 _RcpPoolDim_XY ;
float4 _MinEntryPos_Noise ;
float4 _IndicesDim_IndexChunkSize ;
float4 _Biases_NormalizationClamp ;
float4 _LeakReduction_SkyOcclusion ;
float4 _MaxLoadedCellInEntries_FrameIndex ;
} ;
//#line 9 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/Runtime/Lighting/ProbeVolume/DecodeSH.hlsl"
float3 DecodeSH ( float l0 , float3 l1 )
{
return ( l1 - 0.5f ) * ( 2.0f * 2.0 * l0 ) ;
}

void DecodeSH_L2 ( inout float3 l0 , inout float4 l2_R , inout float4 l2_G , inout float4 l2_B , inout float3 l2_C )
{
l2_R = ( l2_R - 0.5f ) * ( 3.5777088 * l0 . r ) ;
l2_G = ( l2_G - 0.5f ) * ( 3.5777088 * l0 . g ) ;
l2_B = ( l2_B - 0.5f ) * ( 3.5777088 * l0 . b ) ;
l2_C = ( l2_C - 0.5f ) * 3.5777088 ;

l2_C . rgb *= l0 ;
//#line 24
l0 . r -= l2_R . z ;
l0 . g -= l2_G . z ;
l0 . b -= l2_B . z ;
l2_R . z *= 3.0f ;
l2_G . z *= 3.0f ;
l2_B . z *= 3.0f ;
}

void DecodeSH_L2 ( inout float3 l0 , inout float4 l2_R , inout float4 l2_G , inout float4 l2_B , inout float4 l2_C )
{
float3 outL2_C = l2_C . xyz ;
DecodeSH_L2 ( l0 , l2_R , l2_G , l2_B , outL2_C ) ;
l2_C = float4 ( outL2_C . xyz , 0 ) ;
}

half3 DecodeSH ( half l0 , half3 l1 )
{
return ( l1 - 0.5 ) * ( 2.0 * 2.0 * l0 ) ;
}

void DecodeSH_L2 ( inout half3 l0 , inout half4 l2_R , inout half4 l2_G , inout half4 l2_B , inout half3 l2_C )
{
l2_R = ( l2_R - 0.5 ) * ( 3.5777088 * l0 . r ) ;
l2_G = ( l2_G - 0.5 ) * ( 3.5777088 * l0 . g ) ;
l2_B = ( l2_B - 0.5 ) * ( 3.5777088 * l0 . b ) ;
l2_C = ( l2_C - 0.5 ) * 3.5777088 ;

l2_C . rgb *= l0 ;
//#line 54
l0 . r -= l2_R . z ;
l0 . g -= l2_G . z ;
l0 . b -= l2_B . z ;
l2_R . z *= 3.0 ;
l2_G . z *= 3.0 ;
l2_B . z *= 3.0 ;
}

void DecodeSH_L2 ( inout half3 l0 , inout half4 l2_R , inout half4 l2_G , inout half4 l2_B , inout half4 l2_C )
{
half3 outL2_C = l2_C . xyz ;
DecodeSH_L2 ( l0 , l2_R , l2_G , l2_B , outL2_C ) ;
l2_C = half4 ( outL2_C . xyz , 0 ) ;
}

float3 EncodeSH ( float l0 , float3 l1 )
{
return l0 == 0.0f ? 0.5f : l1 * rcp ( l0 ) / ( 2.0f * 2.0 ) + 0.5f ;
}
//#line 81
void EncodeSH_L2 ( inout float3 l0 , inout float4 l2_R , inout float4 l2_G , inout float4 l2_B , inout float3 l2_C )
{

l2_R . z /= 3.0f ;
l2_G . z /= 3.0f ;
l2_B . z /= 3.0f ;
l0 . r += l2_R . z ;
l0 . g += l2_G . z ;
l0 . b += l2_B . z ;

float3 rcpl0 = rcp ( l0 ) ;
rcpl0 = float3 ( l0 . x == 0.0f ? 0.0f : rcpl0 . x , l0 . y == 0.0f ? 0.0f : rcpl0 . y , l0 . z == 0.0f ? 0.0f : rcpl0 . z ) ;

l2_R = 0.5f + l2_R * rcp ( 3.5777088 ) * rcpl0 . r ;
l2_G = 0.5f + l2_G * rcp ( 3.5777088 ) * rcpl0 . g ;
l2_B = 0.5f + l2_B * rcp ( 3.5777088 ) * rcpl0 . b ;
l2_C = 0.5f + l2_C * rcp ( 3.5777088 ) * rcpl0 ;
}
//#line 48 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl"
SamplerState s_linear_clamp_sampler ;
SamplerState s_point_clamp_sampler ;
//#line 55
struct APVResources
{
StructuredBuffer < int > index ;
StructuredBuffer < float3 > SkyPrecomputedDirections ;
//#line 72
Texture3D L0_L1Rx ;

Texture3D L1G_L1Ry ;
Texture3D L1B_L1Rz ;
Texture3D L2_0 ;
Texture3D L2_1 ;
Texture3D L2_2 ;
Texture3D L2_3 ;

Texture3D Validity ;

Texture3D SkyOcclusionL0L1 ;
Texture3D SkyShadingDirectionIndices ;
} ;

struct APVResourcesRW
{
RWTexture3D < float4 > L0_L1Rx ;
RWTexture3D < float4 > L1G_L1Ry ;
RWTexture3D < float4 > L1B_L1Rz ;
RWTexture3D < float4 > L2_0 ;
RWTexture3D < float4 > L2_1 ;
RWTexture3D < float4 > L2_2 ;
RWTexture3D < float4 > L2_3 ;
} ;
//#line 116
struct APVSample
{
half3 L0 ;
half3 L1_R ;
half3 L1_G ;
half3 L1_B ;
//#line 129
float4 skyOcclusionL0L1 ;
float3 skyShadingDirection ;
//#line 136
int status ;
//#line 140
void Decode ( )
{
if ( status == 0 )
{
L1_R = DecodeSH ( L0 . r , L1_R ) ;
L1_G = DecodeSH ( L0 . g , L1_G ) ;
L1_B = DecodeSH ( L0 . b , L1_B ) ;
//#line 151
status = 1 ;
}
}

void Encode ( )
{
if ( status == 1 )
{
L1_R = EncodeSH ( L0 . r , L1_R ) ;
L1_G = EncodeSH ( L0 . g , L1_G ) ;
L1_B = EncodeSH ( L0 . b , L1_B ) ;
//#line 166
status = 0 ;
}
}
} ;
//#line 172
StructuredBuffer < int > _APVResIndex ;
StructuredBuffer < uint3 > _APVResCellIndices ;
StructuredBuffer < float3 > _SkyPrecomputedDirections ;
//#line 188
Texture3D _APVResL0_L1Rx ;

Texture3D _APVResL1G_L1Ry ;
Texture3D _APVResL1B_L1Rz ;
Texture3D _APVResL2_0 ;
Texture3D _APVResL2_1 ;
Texture3D _APVResL2_2 ;
Texture3D _APVResL2_3 ;

Texture3D _APVResValidity ;

Texture3D _SkyOcclusionTexL0L1 ;
Texture3D _SkyShadingDirectionIndicesTex ;
//#line 206
float3 AddNoiseToSamplingPosition ( float3 posWS , float2 positionSS , float3 direction )
{

float3 right = mul ( ( float3x3 ) GetViewToWorldMatrix ( ) , float3 ( 1.0 , 0.0 , 0.0 ) ) ;
float3 top = mul ( ( float3x3 ) GetViewToWorldMatrix ( ) , float3 ( 0.0 , 1.0 , 0.0 ) ) ;
float noise01 = InterleavedGradientNoise ( positionSS , _MaxLoadedCellInEntries_FrameIndex . w ) ;
float noise02 = frac ( noise01 * 100.0 ) ;
float noise03 = frac ( noise01 * 1000.0 ) ;
direction += top * ( noise02 - 0.5 ) + right * ( noise03 - 0.5 ) ;
return _MinEntryPos_Noise . w > 0 ? posWS + noise01 * _MinEntryPos_Noise . w * direction : posWS ;
//#line 219
}

uint3 GetSampleOffset ( uint i )
{
return uint3 ( i , i >> 1 , i >> 2 ) & 1 ;
}
//#line 228
half GetValidityWeight ( uint offset , uint validityMask )
{
uint mask = 1U << offset ;
return ( validityMask & mask ) > 0 ? 1 : 0 ;
}

float ProbeDistance ( uint subdiv )
{
return pow ( 3 , subdiv ) * _PoolDim_MinBrickSize . w / 3.0f ;
}

half ProbeDistanceHalf ( uint subdiv )
{
return pow ( half ( 3 ) , half ( subdiv ) ) * half ( _PoolDim_MinBrickSize . w ) / 3.0 ;
}

float3 GetSnappedProbePosition ( float3 posWS , uint subdiv )
{
float3 distBetweenProbes = ProbeDistance ( subdiv ) ;
float3 dividedPos = posWS / distBetweenProbes ;
return ( dividedPos - frac ( dividedPos ) ) * distBetweenProbes ;
}

float GetNormalWeight ( uint3 offset , float3 posWS , float3 sample0Pos , float3 normalWS , uint subdiv )
{

float3 samplePos = ( sample0Pos - posWS ) + ( float3 ) offset * ProbeDistance ( subdiv ) ;
float3 vecToProbe = normalize ( samplePos ) ;
float weight = saturate ( dot ( vecToProbe , normalWS ) - _LeakReduction_SkyOcclusion . y ) ;
return weight ;
}

half GetNormalWeightHalf ( uint3 offset , float3 posWS , float3 sample0Pos , float3 normalWS , uint subdiv )
{

half3 samplePos = ( half3 ) ( sample0Pos - posWS ) + ( half3 ) offset * ProbeDistanceHalf ( subdiv ) ;
half3 vecToProbe = normalize ( samplePos ) ;
half weight = saturate ( dot ( vecToProbe , ( half3 ) normalWS ) - ( half ) _LeakReduction_SkyOcclusion . y ) ;
return weight ;
}
//#line 273
bool LoadCellIndexMetaData ( int cellFlatIdx , out int chunkIndex , out int stepSize , out int3 minRelativeIdx , out int3 maxRelativeIdxPlusOne )
{
bool cellIsLoaded = false ;
uint3 metaData = _APVResCellIndices [ cellFlatIdx ] ;

if ( metaData . x != 0xFFFFFFFF )
{
chunkIndex = metaData . x & 0x1FFFFFFF ;
stepSize = pow ( 3 , ( metaData . x >> 29 ) & 0x7 ) ;

minRelativeIdx . x = metaData . y & 0x3FF ;
minRelativeIdx . y = ( metaData . y >> 10 ) & 0x3FF ;
minRelativeIdx . z = ( metaData . y >> 20 ) & 0x3FF ;

maxRelativeIdxPlusOne . x = metaData . z & 0x3FF ;
maxRelativeIdxPlusOne . y = ( metaData . z >> 10 ) & 0x3FF ;
maxRelativeIdxPlusOne . z = ( metaData . z >> 20 ) & 0x3FF ;
cellIsLoaded = true ;
}
else
{
chunkIndex = - 1 ;
stepSize = - 1 ;
minRelativeIdx = - 1 ;
maxRelativeIdxPlusOne = - 1 ;
}

return cellIsLoaded ;
}

uint GetIndexData ( APVResources apvRes , float3 posWS )
{
float3 entryPos = floor ( posWS / _Offset_IndirectionEntryDim . w ) ;
float3 topLeftEntryWS = entryPos * _Offset_IndirectionEntryDim . w ;

bool isALoadedCell = all ( entryPos >= _Weight_MinLoadedCellInEntries . yzw ) && all ( entryPos <= _MaxLoadedCellInEntries_FrameIndex . xyz ) ;
//#line 311
int3 entryPosInt = ( int3 ) ( entryPos - _MinEntryPos_Noise . xyz ) ;

int flatIdx = dot ( entryPosInt , int3 ( 1 , ( int ) _IndicesDim_IndexChunkSize . xyz . x , ( ( int ) _IndicesDim_IndexChunkSize . xyz . x * ( int ) _IndicesDim_IndexChunkSize . xyz . y ) ) ) ;

int stepSize = 0 ;
int3 minRelativeIdx , maxRelativeIdxPlusOne ;
int chunkIdx = - 1 ;
bool isValidBrick = false ;
int locationInPhysicalBuffer = 0 ;
//#line 322
[ branch ] if ( isALoadedCell )
{
if ( LoadCellIndexMetaData ( flatIdx , chunkIdx , stepSize , minRelativeIdx , maxRelativeIdxPlusOne ) )
{
float3 residualPosWS = posWS - topLeftEntryWS ;
int3 localBrickIndex = floor ( residualPosWS / ( _PoolDim_MinBrickSize . w * stepSize ) ) ;
//#line 330
isValidBrick = all ( localBrickIndex >= minRelativeIdx ) && all ( localBrickIndex < maxRelativeIdxPlusOne ) ;

int3 sizeOfValid = maxRelativeIdxPlusOne - minRelativeIdx ;

int3 localRelativeIndexLoc = ( localBrickIndex - minRelativeIdx ) ;
int flattenedLocationInCell = dot ( localRelativeIndexLoc , int3 ( sizeOfValid . y , 1 , sizeOfValid . x * sizeOfValid . y ) ) ;

locationInPhysicalBuffer = chunkIdx * ( int ) _IndicesDim_IndexChunkSize . w + flattenedLocationInCell ;
}
}

uint result = 0xffffffff ;
//#line 344
[ branch ] if ( isValidBrick )
{
result = apvRes . index [ locationInPhysicalBuffer ] ;
}

return result ;
}
//#line 355
APVResources FillAPVResources ( )
{
APVResources apvRes ;
apvRes . index = _APVResIndex ;

apvRes . L0_L1Rx = _APVResL0_L1Rx ;

apvRes . L1G_L1Ry = _APVResL1G_L1Ry ;
apvRes . L1B_L1Rz = _APVResL1B_L1Rz ;

apvRes . L2_0 = _APVResL2_0 ;
apvRes . L2_1 = _APVResL2_1 ;
apvRes . L2_2 = _APVResL2_2 ;
apvRes . L2_3 = _APVResL2_3 ;

apvRes . Validity = _APVResValidity ;
apvRes . SkyOcclusionL0L1 = _SkyOcclusionTexL0L1 ;
apvRes . SkyShadingDirectionIndices = _SkyShadingDirectionIndicesTex ;
apvRes . SkyPrecomputedDirections = _SkyPrecomputedDirections ;

return apvRes ;
}
//#line 379
bool TryToGetPoolUVWAndSubdiv ( APVResources apvRes , float3 posWSForSample , out float3 uvw , out uint subdiv )
{

uint packed_pool_idx = GetIndexData ( apvRes , posWSForSample . xyz ) ;
//#line 386
subdiv = ( packed_pool_idx >> 28 ) & 15 ;
float cellSize = pow ( 3.0 , subdiv ) ;

float flattened_pool_idx = packed_pool_idx & ( ( 1 << 28 ) - 1 ) ;
float3 pool_idx ;
pool_idx . z = floor ( flattened_pool_idx * _RcpPoolDim_XY . w ) ;
flattened_pool_idx -= ( pool_idx . z * ( _PoolDim_MinBrickSize . xyz . x * _PoolDim_MinBrickSize . xyz . y ) ) ;
pool_idx . y = floor ( flattened_pool_idx * _RcpPoolDim_XY . xyz . x ) ;
pool_idx . x = floor ( flattened_pool_idx - ( pool_idx . y * _PoolDim_MinBrickSize . xyz . x ) ) ;
//#line 397
float3 posRS = posWSForSample . xyz / _PoolDim_MinBrickSize . w ;
float3 offset = frac ( posRS / ( float ) cellSize ) ;
//#line 401
uvw = ( pool_idx + 0.5 + ( 3.0 * offset ) ) * _RcpPoolDim_XY . xyz ;
//#line 405
return packed_pool_idx != 0xffffffffu ;
}

bool TryToGetPoolUVWAndSubdiv ( APVResources apvRes , float3 posWS , float3 normalWS , float3 viewDirWS , out float3 uvw , out uint subdiv , out float3 biasedPosWS )
{
biasedPosWS = ( posWS + normalWS * _Biases_NormalizationClamp . x ) + viewDirWS * _Biases_NormalizationClamp . y ;
return TryToGetPoolUVWAndSubdiv ( apvRes , biasedPosWS , uvw , subdiv ) ;
}

bool TryToGetPoolUVW ( APVResources apvRes , float3 posWS , float3 normalWS , float3 viewDir , out float3 uvw )
{
uint unusedSubdiv ;
float3 unusedPos ;
return TryToGetPoolUVWAndSubdiv ( apvRes , posWS , normalWS , viewDir , uvw , unusedSubdiv , unusedPos ) ;
}
//#line 422
APVSample SampleAPV ( APVResources apvRes , float3 uvw )
{
APVSample apvSample ;
half4 L0_L1Rx = half4 ( apvRes . L0_L1Rx . SampleLevel ( s_linear_clamp_sampler , uvw , 0 ) . rgba ) ;
half4 L1G_L1Ry = half4 ( apvRes . L1G_L1Ry . SampleLevel ( s_linear_clamp_sampler , uvw , 0 ) . rgba ) ;
half4 L1B_L1Rz = half4 ( apvRes . L1B_L1Rz . SampleLevel ( s_linear_clamp_sampler , uvw , 0 ) . rgba ) ;

apvSample . L0 = L0_L1Rx . xyz ;
apvSample . L1_R = half3 ( L0_L1Rx . w , L1G_L1Ry . w , L1B_L1Rz . w ) ;
apvSample . L1_G = L1G_L1Ry . xyz ;
apvSample . L1_B = L1B_L1Rz . xyz ;
//#line 441
if ( _LeakReduction_SkyOcclusion . z > 0 )
apvSample . skyOcclusionL0L1 = apvRes . SkyOcclusionL0L1 . SampleLevel ( s_linear_clamp_sampler , uvw , 0 ) . rgba ;
else
apvSample . skyOcclusionL0L1 = float4 ( 0 , 0 , 0 , 0 ) ;

if ( _LeakReduction_SkyOcclusion . w > 0 )
{

float3 texCoordFloat = uvw * _PoolDim_MinBrickSize . xyz - 0.5f ;
int3 texCoordInt = texCoordFloat ;
uint index = apvRes . SkyShadingDirectionIndices . Load ( int4 ( texCoordInt , 0 ) ) . x * 255.0 ;

if ( index == 255 )
apvSample . skyShadingDirection = float3 ( 0 , 0 , 0 ) ;
else
apvSample . skyShadingDirection = apvRes . SkyPrecomputedDirections [ index ] . rgb ;
}
else
apvSample . skyShadingDirection = float3 ( 0 , 0 , 0 ) ;

apvSample . status = 0 ;

return apvSample ;
}

APVSample LoadAndDecodeAPV ( APVResources apvRes , int3 loc )
{
APVSample apvSample ;

half4 L0_L1Rx = half4 ( apvRes . L0_L1Rx . Load ( int4 ( loc , 0 ) ) . rgba ) ;
half4 L1G_L1Ry = half4 ( apvRes . L1G_L1Ry . Load ( int4 ( loc , 0 ) ) . rgba ) ;
half4 L1B_L1Rz = half4 ( apvRes . L1B_L1Rz . Load ( int4 ( loc , 0 ) ) . rgba ) ;

apvSample . L0 = L0_L1Rx . xyz ;
apvSample . L1_R = half3 ( L0_L1Rx . w , L1G_L1Ry . w , L1B_L1Rz . w ) ;
apvSample . L1_G = L1G_L1Ry . xyz ;
apvSample . L1_B = L1B_L1Rz . xyz ;
//#line 486
apvSample . status = 0 ;
apvSample . Decode ( ) ;

return apvSample ;
}

void WeightSample ( inout APVSample apvSample , half weight )
{
apvSample . L0 *= weight ;
apvSample . L1_R *= weight ;
apvSample . L1_G *= weight ;
apvSample . L1_B *= weight ;
//#line 505
}

void AccumulateSamples ( inout APVSample dst , APVSample other , half weight )
{
WeightSample ( other , weight ) ;
dst . L0 += other . L0 ;
dst . L1_R += other . L1_R ;
dst . L1_G += other . L1_G ;
dst . L1_B += other . L1_B ;
//#line 521
}

APVSample ManuallyFilteredSample ( APVResources apvRes , float3 posWS , float3 normalWS , int subdiv , float3 biasedPosWS , float3 uvw )
{
float3 texCoordFloat = uvw * _PoolDim_MinBrickSize . xyz - .5f ;
int3 texCoordInt = texCoordFloat ;
float3 texFrac = frac ( texCoordFloat ) ;
float3 oneMinTexFrac = 1.0f - texFrac ;

bool sampled = false ;
float totalW = 0.0f ;

APVSample baseSample ;

float3 positionCentralProbe = GetSnappedProbePosition ( biasedPosWS , subdiv ) ;

baseSample = ( APVSample ) 0 ; ;

uint validityMask = apvRes . Validity . Load ( int4 ( texCoordInt , 0 ) ) . x * 255.0 ;
for ( uint i = 0 ; i < 8 ; ++ i )
{
uint3 offset = GetSampleOffset ( i ) ;
float trilinearW =
( ( offset . x == 1 ) ? texFrac . x : oneMinTexFrac . x ) *
( ( offset . y == 1 ) ? texFrac . y : oneMinTexFrac . y ) *
( ( offset . z == 1 ) ? texFrac . z : oneMinTexFrac . z ) ;

half validityWeight = GetValidityWeight ( i , validityMask ) ;

if ( validityWeight > 0 )
{
APVSample apvSample = LoadAndDecodeAPV ( apvRes , texCoordInt + offset ) ;
half geoW = GetNormalWeightHalf ( offset , posWS , positionCentralProbe , normalWS , subdiv ) ;

half finalW = half ( geoW * trilinearW ) ;
AccumulateSamples ( baseSample , apvSample , finalW ) ;
totalW += finalW ;
}
}

WeightSample ( baseSample , half ( rcp ( totalW ) ) ) ;

return baseSample ;
}

void WarpUVWLeakReduction ( APVResources apvRes , float3 posWS , float3 normalWS , uint subdiv , float3 biasedPosWS , inout float3 uvw , out float3 normalizedOffset , out float validityWeights [ 8 ] )
{
float3 texCoordFloat = uvw * _PoolDim_MinBrickSize . xyz - 0.5f ;
int3 texCoordInt = texCoordFloat ;
half3 texFrac = half3 ( frac ( texCoordFloat ) ) ;
half3 oneMinTexFrac = 1.0 - texFrac ;
uint validityMask = apvRes . Validity . Load ( int4 ( texCoordInt , 0 ) ) . x * 255.0 ;

half4 weights [ 2 ] ;
half totalW = 0.0 ;
uint i = 0 ;
float3 positionCentralProbe = GetSnappedProbePosition ( biasedPosWS , subdiv ) ;

[ unroll ]
for ( i = 0 ; i < 8 ; ++ i )
{
uint3 offset = GetSampleOffset ( i ) ;
half trilinearW =
( ( offset . x == 1 ) ? texFrac . x : oneMinTexFrac . x ) *
( ( offset . y == 1 ) ? texFrac . y : oneMinTexFrac . y ) *
( ( offset . z == 1 ) ? texFrac . z : oneMinTexFrac . z ) ;

half validityWeight = GetValidityWeight ( i , validityMask ) ;
validityWeights [ i ] = validityWeight ;

half geoW = GetNormalWeightHalf ( offset , posWS , positionCentralProbe , normalWS , subdiv ) ;

half weight = saturate ( trilinearW * ( geoW * validityWeight ) ) ;

weights [ i / 4 ] [ i % 4 ] = weight ;
totalW += weight ;
}

half rcpTotalW = rcp ( max ( 0.0001 , totalW ) ) ;
weights [ 0 ] *= rcpTotalW ;
weights [ 1 ] *= rcpTotalW ;

half3 fracOffset = - texFrac ;

[ unroll ]
for ( i = 0 ; i < 8 ; ++ i )
{
uint3 offset = GetSampleOffset ( i ) ;
fracOffset += ( half3 ) offset * weights [ i / 4 ] [ i % 4 ] ;
}

normalizedOffset = ( float3 ) ( fracOffset + texFrac ) ;

uvw = uvw + ( float3 ) fracOffset * _RcpPoolDim_XY . xyz ;
}

void WarpUVWLeakReduction ( APVResources apvRes , float3 posWS , float3 normalWS , uint subdiv , float3 biasedPosWS , inout float3 uvw )
{
float3 normalizedOffset ;
float validityWeights [ 8 ] ;
WarpUVWLeakReduction ( apvRes , posWS , normalWS , subdiv , biasedPosWS , uvw , normalizedOffset , validityWeights ) ;
}

APVSample SampleAPV ( APVResources apvRes , float3 posWS , float3 biasNormalWS , float3 viewDir )
{
APVSample outSample ;

posWS -= _Offset_IndirectionEntryDim . xyz ;

float3 pool_uvw ;
uint subdiv ;
float3 biasedPosWS ;
if ( TryToGetPoolUVWAndSubdiv ( apvRes , posWS , biasNormalWS , viewDir , pool_uvw , subdiv , biasedPosWS ) )
{
//#line 641
if ( _LeakReduction_SkyOcclusion . x != 0 )
{
WarpUVWLeakReduction ( apvRes , posWS , biasNormalWS , subdiv , biasedPosWS , pool_uvw ) ;
}
outSample = SampleAPV ( apvRes , pool_uvw ) ;

}
else
{
outSample = ( APVSample ) 0 ; ;
outSample . status = - 1 ;
}

return outSample ;
}
//#line 658
APVSample SampleAPV ( float3 posWS , float3 biasNormalWS , float3 viewDir )
{
APVResources apvRes = FillAPVResources ( ) ;
return SampleAPV ( apvRes , posWS , biasNormalWS , viewDir ) ;
}
//#line 670
float EvalSHSkyOcclusion ( float3 dir , APVSample apvSample )
{

float4 temp = float4 ( 0.28209479177387814347f , 0.48860251190291992159f * dir . x , 0.48860251190291992159f * dir . y , 0.48860251190291992159f * dir . z ) ;
return _LeakReduction_SkyOcclusion . z * dot ( temp , apvSample . skyOcclusionL0L1 ) ;
}

float3 EvaluateOccludedSky ( APVSample apvSample , float3 N )
{
float occValue = EvalSHSkyOcclusion ( N , apvSample ) ;
float3 shadingNormal = N ;

if ( _LeakReduction_SkyOcclusion . w > 0 )
{
shadingNormal = apvSample . skyShadingDirection ;
float normSquared = dot ( shadingNormal , shadingNormal ) ;
if ( normSquared < 0.2f )
shadingNormal = N ;
else
{
shadingNormal = shadingNormal * rsqrt ( normSquared ) ;
}
}
return occValue * EvaluateAmbientProbe ( shadingNormal ) ;
}
//#line 699
float3 EvaluateAPVL0 ( APVSample apvSample )
{
return apvSample . L0 ;
}

void EvaluateAPVL1 ( APVSample apvSample , float3 N , out float3 diffuseLighting )
{
diffuseLighting = SHEvalLinearL1 ( N , apvSample . L1_R , apvSample . L1_G , apvSample . L1_B ) ;
}
//#line 721
void EvaluateAdaptiveProbeVolume ( APVSample apvSample , float3 normalWS , out float3 bakeDiffuseLighting )
{
if ( apvSample . status != - 1 )
{
apvSample . Decode ( ) ;
//#line 728
EvaluateAPVL1 ( apvSample , normalWS , bakeDiffuseLighting ) ;
//#line 733
bakeDiffuseLighting += apvSample . L0 ;
if ( _LeakReduction_SkyOcclusion . z > 0 )
bakeDiffuseLighting += EvaluateOccludedSky ( apvSample , normalWS ) ;
//#line 738
{
bakeDiffuseLighting = bakeDiffuseLighting * _Weight_MinLoadedCellInEntries . x ;
}
}
else
{

bakeDiffuseLighting = EvaluateAmbientProbe ( normalWS ) ;
}
}

void EvaluateAdaptiveProbeVolume ( APVSample apvSample , float3 normalWS , float3 backNormalWS , out float3 bakeDiffuseLighting , out float3 backBakeDiffuseLighting )
{
if ( apvSample . status != - 1 )
{
apvSample . Decode ( ) ;
//#line 756
EvaluateAPVL1 ( apvSample , normalWS , bakeDiffuseLighting ) ;
EvaluateAPVL1 ( apvSample , backNormalWS , backBakeDiffuseLighting ) ;
//#line 763
bakeDiffuseLighting += apvSample . L0 ;
backBakeDiffuseLighting += apvSample . L0 ;
if ( _LeakReduction_SkyOcclusion . z > 0 )
{
bakeDiffuseLighting += EvaluateOccludedSky ( apvSample , normalWS ) ;
backBakeDiffuseLighting += EvaluateOccludedSky ( apvSample , backNormalWS ) ;
}
//#line 772
{
bakeDiffuseLighting = bakeDiffuseLighting * _Weight_MinLoadedCellInEntries . x ;
backBakeDiffuseLighting = backBakeDiffuseLighting * _Weight_MinLoadedCellInEntries . x ;
}
}
else
{

bakeDiffuseLighting = EvaluateAmbientProbe ( normalWS ) ;
backBakeDiffuseLighting = EvaluateAmbientProbe ( backNormalWS ) ;
}
}

void EvaluateAdaptiveProbeVolume ( in float3 posWS , in float3 normalWS , in float3 backNormalWS , in float3 reflDir , in float3 viewDir ,
in float2 positionSS , out float3 bakeDiffuseLighting , out float3 backBakeDiffuseLighting , out float3 lightingInReflDir )
{
APVResources apvRes = FillAPVResources ( ) ;

posWS = AddNoiseToSamplingPosition ( posWS , positionSS , viewDir ) ;

APVSample apvSample = SampleAPV ( posWS , normalWS , viewDir ) ;

if ( apvSample . status != - 1 )
{

apvSample . Decode ( ) ;
//#line 801
EvaluateAPVL1 ( apvSample , normalWS , bakeDiffuseLighting ) ;
EvaluateAPVL1 ( apvSample , backNormalWS , backBakeDiffuseLighting ) ;
EvaluateAPVL1 ( apvSample , reflDir , lightingInReflDir ) ;
//#line 810
bakeDiffuseLighting += apvSample . L0 ;
backBakeDiffuseLighting += apvSample . L0 ;
lightingInReflDir += apvSample . L0 ;
if ( _LeakReduction_SkyOcclusion . z > 0 )
{
bakeDiffuseLighting += EvaluateOccludedSky ( apvSample , normalWS ) ;
backBakeDiffuseLighting += EvaluateOccludedSky ( apvSample , backNormalWS ) ;
lightingInReflDir += EvaluateOccludedSky ( apvSample , reflDir ) ;
}
//#line 821
{
bakeDiffuseLighting = bakeDiffuseLighting * _Weight_MinLoadedCellInEntries . x ;
backBakeDiffuseLighting = backBakeDiffuseLighting * _Weight_MinLoadedCellInEntries . x ;
}
}
else
{
bakeDiffuseLighting = EvaluateAmbientProbe ( normalWS ) ;
backBakeDiffuseLighting = EvaluateAmbientProbe ( backNormalWS ) ;
lightingInReflDir = - 1 ;
}
}

void EvaluateAdaptiveProbeVolume ( in float3 posWS , in float3 normalWS , in float3 backNormalWS , in float3 viewDir ,
in float2 positionSS , out float3 bakeDiffuseLighting , out float3 backBakeDiffuseLighting )
{
bakeDiffuseLighting = float3 ( 0.0 , 0.0 , 0.0 ) ;
backBakeDiffuseLighting = float3 ( 0.0 , 0.0 , 0.0 ) ;

posWS = AddNoiseToSamplingPosition ( posWS , positionSS , viewDir ) ;

APVSample apvSample = SampleAPV ( posWS , normalWS , viewDir ) ;
EvaluateAdaptiveProbeVolume ( apvSample , normalWS , backNormalWS , bakeDiffuseLighting , backBakeDiffuseLighting ) ;
}

void EvaluateAdaptiveProbeVolume ( in float3 posWS , in float3 normalWS , in float3 viewDir , in float2 positionSS ,
out float3 bakeDiffuseLighting )
{
bakeDiffuseLighting = float3 ( 0.0 , 0.0 , 0.0 ) ;

posWS = AddNoiseToSamplingPosition ( posWS , positionSS , viewDir ) ;

APVSample apvSample = SampleAPV ( posWS , normalWS , viewDir ) ;
EvaluateAdaptiveProbeVolume ( apvSample , normalWS , bakeDiffuseLighting ) ;
}

void EvaluateAdaptiveProbeVolume ( in float3 posWS , in float2 positionSS , out float3 bakeDiffuseLighting )
{
APVResources apvRes = FillAPVResources ( ) ;

posWS = AddNoiseToSamplingPosition ( posWS , positionSS , 1 ) ;
posWS -= _Offset_IndirectionEntryDim . xyz ;

float3 uvw ;
if ( TryToGetPoolUVW ( apvRes , posWS , 0 , 0 , uvw ) )
{
bakeDiffuseLighting = apvRes . L0_L1Rx . SampleLevel ( s_linear_clamp_sampler , uvw , 0 ) . rgb ;
}
else
{
bakeDiffuseLighting = EvaluateAmbientProbe ( 0 ) ;
}
}
//#line 880
float EvaluateReflectionProbeSH ( float3 sampleDir , float4 reflProbeSHL0L1 , float4 reflProbeSHL2_1 , float reflProbeSHL2_2 )
{
float outFactor = 0 ;
float L0 = reflProbeSHL0L1 . x ;
float L1 = dot ( reflProbeSHL0L1 . yzw , sampleDir ) ;

outFactor = L0 + L1 ;
//#line 901
return outFactor ;
}

float GetReflectionProbeNormalizationFactor ( float3 lightingInReflDir , float3 sampleDir , float4 reflProbeSHL0L1 , float4 reflProbeSHL2_1 , float reflProbeSHL2_2 )
{
float refProbeNormalization = EvaluateReflectionProbeSH ( sampleDir , reflProbeSHL0L1 , reflProbeSHL2_1 , reflProbeSHL2_2 ) ;

float localNormalization = Luminance ( float3 ( lightingInReflDir ) ) ;
return lerp ( 1.f , clamp ( SafeDiv ( localNormalization , refProbeNormalization ) , _Biases_NormalizationClamp . z , _Biases_NormalizationClamp . w ) , _Weight_MinLoadedCellInEntries . x ) ;

}
//#line 29 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
half3 SampleSHVertex ( half3 normalWS )
{
//#line 39
return half3 ( 0.0 , 0.0 , 0.0 ) ;
}
//#line 44
half3 SampleSHPixel ( half3 L2Term , half3 normalWS )
{
//#line 57
return EvaluateAmbientProbeSRGB ( normalWS ) ;
}
//#line 64
half3 SampleProbeVolumeVertex ( in float3 absolutePositionWS , in float3 normalWS , in float3 viewDir )
{
//#line 83
return half3 ( 0 , 0 , 0 ) ;

}

half3 SampleProbeVolumePixel ( in half3 vertexValue , in float3 absolutePositionWS , in float3 normalWS , in float3 viewDir , in float2 positionSS )
{
//#line 92
half3 bakedGI ;
if ( _EnableProbeVolumes )
{
EvaluateAdaptiveProbeVolume ( absolutePositionWS , normalWS , viewDir , positionSS , bakedGI ) ;
}
else
{
bakedGI = EvaluateAmbientProbe ( normalWS ) ;
}
//#line 104
return bakedGI ;
//#line 108
}
//#line 111
half3 SampleProbeSHVertex ( in float3 absolutePositionWS , in float3 normalWS , in float3 viewDir )
{

return SampleProbeVolumeVertex ( absolutePositionWS , normalWS , viewDir ) ;
//#line 118
}
//#line 139
half3 SampleLightmap ( float2 staticLightmapUV , float2 dynamicLightmapUV , half3 normalWS )
{
//#line 144
half4 transformCoords = half4 ( 1 , 1 , 0 , 0 ) ;

float3 diffuseLighting = 0 ;
//#line 165
return diffuseLighting ;
}
//#line 169
half3 SampleLightmap ( float2 staticLightmapUV , half3 normalWS )
{
float2 dummyDynamicLightmapUV = float2 ( 0 , 0 ) ;
half3 result = SampleLightmap ( staticLightmapUV , dummyDynamicLightmapUV , normalWS ) ;
return result ;
}
//#line 191
half3 BoxProjectedCubemapDirection ( half3 reflectionWS , float3 positionWS , float4 cubemapPositionWS , float4 boxMin , float4 boxMax )
{

if ( cubemapPositionWS . w > 0.0f )
{
float3 boxMinMax = ( reflectionWS > 0.0f ) ? boxMax . xyz : boxMin . xyz ;
half3 rbMinMax = half3 ( boxMinMax - positionWS ) / reflectionWS ;

half fa = half ( min ( min ( rbMinMax . x , rbMinMax . y ) , rbMinMax . z ) ) ;

half3 worldPos = half3 ( positionWS - cubemapPositionWS . xyz ) ;

half3 result = worldPos + reflectionWS * fa ;
return result ;
}
else
{
return reflectionWS ;
}
}

float CalculateProbeWeight ( float3 positionWS , float4 probeBoxMin , float4 probeBoxMax )
{
float blendDistance = probeBoxMax . w ;
float3 weightDir = min ( positionWS - probeBoxMin . xyz , probeBoxMax . xyz - positionWS ) / blendDistance ;
return saturate ( min ( weightDir . x , min ( weightDir . y , weightDir . z ) ) ) ;
}

half CalculateProbeVolumeSqrMagnitude ( float4 probeBoxMin , float4 probeBoxMax )
{
half3 maxToMin = half3 ( probeBoxMax . xyz - probeBoxMin . xyz ) ;
return dot ( maxToMin , maxToMin ) ;
}

half3 CalculateIrradianceFromReflectionProbes ( half3 reflectVector , float3 positionWS , half perceptualRoughness , float2 normalizedScreenSpaceUV )
{
half3 irradiance = half3 ( 0.0h , 0.0h , 0.0h ) ;
half mip = PerceptualRoughnessToMipmapLevel ( perceptualRoughness ) ;

float totalWeight = 0.0f ;
uint probeIndex ;
ClusterIterator it = ClusterInit ( normalizedScreenSpaceUV , positionWS , 1 ) ;
[ loop ] while ( ClusterNext ( it , probeIndex ) && totalWeight < 0.99f )
{
probeIndex -= ( ( uint ) _FPParams0 . z ) ;

float weight = CalculateProbeWeight ( positionWS , urp_ReflProbes_BoxMin [ probeIndex ] , urp_ReflProbes_BoxMax [ probeIndex ] ) ;
weight = min ( weight , 1.0f - totalWeight ) ;

half3 sampleVector = reflectVector ;

sampleVector = BoxProjectedCubemapDirection ( reflectVector , positionWS , urp_ReflProbes_ProbePosition [ probeIndex ] , urp_ReflProbes_BoxMin [ probeIndex ] , urp_ReflProbes_BoxMax [ probeIndex ] ) ;
//#line 245
uint maxMip = ( uint ) abs ( urp_ReflProbes_ProbePosition [ probeIndex ] . w ) - 1 ;
half probeMip = min ( mip , maxMip ) ;
float2 uv = saturate ( PackNormalOctQuadEncode ( sampleVector ) * 0.5 + 0.5 ) ;

float mip0 = floor ( probeMip ) ;
float mip1 = mip0 + 1 ;
float mipBlend = probeMip - mip0 ;
float4 scaleOffset0 = urp_ReflProbes_MipScaleOffset [ probeIndex * 7 + ( uint ) mip0 ] ;
float4 scaleOffset1 = urp_ReflProbes_MipScaleOffset [ probeIndex * 7 + ( uint ) mip1 ] ;

half3 irradiance0 = half4 ( urp_ReflProbes_Atlas . SampleLevel ( sampler_LinearClamp , uv * scaleOffset0 . xy + scaleOffset0 . zw , 0.0 ) ) . rgb ;
half3 irradiance1 = half4 ( urp_ReflProbes_Atlas . SampleLevel ( sampler_LinearClamp , uv * scaleOffset1 . xy + scaleOffset1 . zw , 0.0 ) ) . rgb ;
irradiance += weight * lerp ( irradiance0 , irradiance1 , mipBlend ) ;
totalWeight += weight ;
}
//#line 313
if ( totalWeight < 0.99f )
{
half4 encodedIrradiance = half4 ( _GlossyEnvironmentCubeMap . SampleLevel ( sampler_GlossyEnvironmentCubeMap , reflectVector , mip ) ) ;

irradiance += ( 1.0f - totalWeight ) * DecodeHDREnvironment ( encodedIrradiance , _GlossyEnvironmentCubeMap_HDR ) ;
}

return irradiance ;
}
//#line 330
half3 GlossyEnvironmentReflection ( half3 reflectVector , float3 positionWS , half perceptualRoughness , half occlusion , float2 normalizedScreenSpaceUV )
{

half3 irradiance ;
//#line 336
irradiance = CalculateIrradianceFromReflectionProbes ( reflectVector , positionWS , perceptualRoughness , normalizedScreenSpaceUV ) ;
//#line 346
return irradiance * occlusion ;
//#line 350
}
//#line 359
half3 GlossyEnvironmentReflection ( half3 reflectVector , half perceptualRoughness , half occlusion )
{

half3 irradiance ;
half mip = PerceptualRoughnessToMipmapLevel ( perceptualRoughness ) ;
half4 encodedIrradiance = half4 ( unity_SpecCube0 . SampleLevel ( samplerunity_SpecCube0 , reflectVector , mip ) ) ;

irradiance = DecodeHDREnvironment ( encodedIrradiance , unity_SpecCube0_HDR ) ;

return irradiance * occlusion ;
//#line 373
}

half3 SubtractDirectMainLightFromLightmap ( Light mainLight , half3 normalWS , half3 bakedGI )
{
//#line 389
half shadowStrength = GetMainLightShadowStrength ( ) ;
half contributionTerm = saturate ( dot ( mainLight . direction , normalWS ) ) ;
half3 lambert = mainLight . color * contributionTerm ;
half3 estimatedLightContributionMaskedByInverseOfShadow = lambert * ( 1.0 - mainLight . shadowAttenuation ) ;
half3 subtractedLightmap = bakedGI - estimatedLightContributionMaskedByInverseOfShadow ;
//#line 396
half3 realtimeShadow = max ( subtractedLightmap , _SubtractiveShadowColor . xyz ) ;
realtimeShadow = lerp ( bakedGI , realtimeShadow , shadowStrength ) ;
//#line 400
return min ( bakedGI , realtimeShadow ) ;
}

half3 GlobalIllumination ( BRDFData brdfData , BRDFData brdfDataClearCoat , float clearCoatMask ,
half3 bakedGI , half occlusion , float3 positionWS ,
half3 normalWS , half3 viewDirectionWS , float2 normalizedScreenSpaceUV )
{
half3 reflectVector = reflect ( - viewDirectionWS , normalWS ) ;
half NoV = saturate ( dot ( normalWS , viewDirectionWS ) ) ;
half fresnelTerm = Pow4 ( 1.0 - NoV ) ;

half3 indirectDiffuse = bakedGI ;
half3 indirectSpecular = GlossyEnvironmentReflection ( reflectVector , positionWS , brdfData . perceptualRoughness , 1.0h , normalizedScreenSpaceUV ) ;

half3 color = EnvironmentBRDF ( brdfData , indirectDiffuse , indirectSpecular , fresnelTerm ) ;

if ( IsOnlyAOLightingFeatureEnabled ( ) )
{
color = half3 ( 1 , 1 , 1 ) ;
}
//#line 432
return color * occlusion ;

}
//#line 446
half3 GlobalIllumination ( BRDFData brdfData , half3 bakedGI , half occlusion , float3 positionWS , half3 normalWS , half3 viewDirectionWS )
{
const BRDFData noClearCoat = ( BRDFData ) 0 ;
return GlobalIllumination ( brdfData , noClearCoat , 0.0 , bakedGI , occlusion , positionWS , normalWS , viewDirectionWS , 0 ) ;
}

half3 GlobalIllumination ( BRDFData brdfData , BRDFData brdfDataClearCoat , float clearCoatMask ,
half3 bakedGI , half occlusion ,
half3 normalWS , half3 viewDirectionWS )
{
half3 reflectVector = reflect ( - viewDirectionWS , normalWS ) ;
half NoV = saturate ( dot ( normalWS , viewDirectionWS ) ) ;
half fresnelTerm = Pow4 ( 1.0 - NoV ) ;

half3 indirectDiffuse = bakedGI ;
half3 indirectSpecular = GlossyEnvironmentReflection ( reflectVector , brdfData . perceptualRoughness , half ( 1.0 ) ) ;

half3 color = EnvironmentBRDF ( brdfData , indirectDiffuse , indirectSpecular , fresnelTerm ) ;
//#line 476
return color * occlusion ;

}
//#line 481
half3 GlobalIllumination ( BRDFData brdfData , half3 bakedGI , half occlusion , half3 normalWS , half3 viewDirectionWS )
{
const BRDFData noClearCoat = ( BRDFData ) 0 ;
return GlobalIllumination ( brdfData , noClearCoat , 0.0 , bakedGI , occlusion , normalWS , viewDirectionWS ) ;
}

void MixRealtimeAndBakedGI ( inout Light light , half3 normalWS , inout half3 bakedGI )
{
//#line 492
}
//#line 495
void MixRealtimeAndBakedGI ( inout Light light , half3 normalWS , inout half3 bakedGI , half4 shadowMask )
{
MixRealtimeAndBakedGI ( light , normalWS , bakedGI ) ;
}

void MixRealtimeAndBakedGI ( inout Light light , half3 normalWS , inout half3 bakedGI , AmbientOcclusionFactor aoFactor )
{
if ( IsLightingFeatureEnabled ( ( 32 ) ) )
{
bakedGI *= aoFactor . indirectAmbientOcclusion ;
}

MixRealtimeAndBakedGI ( light , normalWS , bakedGI ) ;
}
//#line 29 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
half3 LightingLambert ( half3 lightColor , half3 lightDir , half3 normal )
{
half NdotL = saturate ( dot ( normal , lightDir ) ) ;
return lightColor * NdotL ;
}

half3 LightingSpecular ( half3 lightColor , half3 lightDir , half3 normal , half3 viewDir , half4 specular , half smoothness )
{
float3 halfVec = SafeNormalize ( float3 ( lightDir ) + float3 ( viewDir ) ) ;
half NdotH = half ( saturate ( dot ( normal , halfVec ) ) ) ;
half modifier = pow ( float ( NdotH ) , float ( smoothness ) ) ;
half3 specularReflection = specular . rgb * modifier ;
return lightColor * specularReflection ;
}

half3 LightingPhysicallyBased ( BRDFData brdfData , BRDFData brdfDataClearCoat ,
half3 lightColor , half3 lightDirectionWS , float lightAttenuation ,
half3 normalWS , half3 viewDirectionWS ,
half clearCoatMask , bool specularHighlightsOff )
{
half NdotL = saturate ( dot ( normalWS , lightDirectionWS ) ) ;
half3 radiance = lightColor * ( lightAttenuation * NdotL ) ;

half3 brdf = brdfData . diffuse ;

[ branch ] if ( ! specularHighlightsOff )
{
brdf += brdfData . specular * DirectBRDFSpecular ( brdfData , normalWS , lightDirectionWS , viewDirectionWS ) ;
//#line 73
}
//#line 76
return brdf * radiance ;
}

half3 LightingPhysicallyBased ( BRDFData brdfData , BRDFData brdfDataClearCoat , Light light , half3 normalWS , half3 viewDirectionWS , half clearCoatMask , bool specularHighlightsOff )
{
return LightingPhysicallyBased ( brdfData , brdfDataClearCoat , light . color , light . direction , light . distanceAttenuation * light . shadowAttenuation , normalWS , viewDirectionWS , clearCoatMask , specularHighlightsOff ) ;
}
//#line 85
half3 LightingPhysicallyBased ( BRDFData brdfData , Light light , half3 normalWS , half3 viewDirectionWS )
{
//#line 90
bool specularHighlightsOff = false ;

const BRDFData noClearCoat = ( BRDFData ) 0 ;
return LightingPhysicallyBased ( brdfData , noClearCoat , light , normalWS , viewDirectionWS , 0.0 , specularHighlightsOff ) ;
}

half3 LightingPhysicallyBased ( BRDFData brdfData , half3 lightColor , half3 lightDirectionWS , float lightAttenuation , half3 normalWS , half3 viewDirectionWS )
{
Light light ;
light . color = lightColor ;
light . direction = lightDirectionWS ;
light . distanceAttenuation = lightAttenuation ;
light . shadowAttenuation = 1 ;
return LightingPhysicallyBased ( brdfData , light , normalWS , viewDirectionWS ) ;
}

half3 LightingPhysicallyBased ( BRDFData brdfData , Light light , half3 normalWS , half3 viewDirectionWS , bool specularHighlightsOff )
{
const BRDFData noClearCoat = ( BRDFData ) 0 ;
return LightingPhysicallyBased ( brdfData , noClearCoat , light , normalWS , viewDirectionWS , 0.0 , specularHighlightsOff ) ;
}

half3 LightingPhysicallyBased ( BRDFData brdfData , half3 lightColor , half3 lightDirectionWS , float lightAttenuation , half3 normalWS , half3 viewDirectionWS , bool specularHighlightsOff )
{
Light light ;
light . color = lightColor ;
light . direction = lightDirectionWS ;
light . distanceAttenuation = lightAttenuation ;
light . shadowAttenuation = 1 ;
return LightingPhysicallyBased ( brdfData , light , viewDirectionWS , specularHighlightsOff , specularHighlightsOff ) ;
}

half3 VertexLighting ( float3 positionWS , half3 normalWS )
{
half3 vertexLightColor = half3 ( 0.0 , 0.0 , 0.0 ) ;
//#line 144
return vertexLightColor ;
}

struct LightingData
{
half3 giColor ;
half3 mainLightColor ;
half3 additionalLightsColor ;
half3 vertexLightingColor ;
half3 emissionColor ;
} ;

half3 CalculateLightingColor ( LightingData lightingData , half3 albedo )
{
half3 lightingColor = 0 ;

if ( IsOnlyAOLightingFeatureEnabled ( ) )
{
return lightingData . giColor ;
}

if ( IsLightingFeatureEnabled ( ( 1 ) ) )
{
lightingColor += lightingData . giColor ;
}

if ( IsLightingFeatureEnabled ( ( 2 ) ) )
{
lightingColor += lightingData . mainLightColor ;
}

if ( IsLightingFeatureEnabled ( ( 4 ) ) )
{
lightingColor += lightingData . additionalLightsColor ;
}

if ( IsLightingFeatureEnabled ( ( 8 ) ) )
{
lightingColor += lightingData . vertexLightingColor ;
}

lightingColor *= albedo ;

if ( IsLightingFeatureEnabled ( ( 16 ) ) )
{
lightingColor += lightingData . emissionColor ;
}

return lightingColor ;
}

half4 CalculateFinalColor ( LightingData lightingData , half alpha )
{
half3 finalColor = CalculateLightingColor ( lightingData , 1 ) ;

return half4 ( finalColor , alpha ) ;
}

half4 CalculateFinalColor ( LightingData lightingData , half3 albedo , half alpha , float fogCoord )
{
//#line 210
half fogFactor = 0 ;
//#line 215
half3 lightingColor = CalculateLightingColor ( lightingData , albedo ) ;
half3 finalColor = MixFog ( lightingColor , fogFactor ) ;

return half4 ( finalColor , alpha ) ;
}

LightingData CreateLightingData ( InputData inputData , SurfaceData surfaceData )
{
LightingData lightingData ;

lightingData . giColor = inputData . bakedGI ;
lightingData . emissionColor = surfaceData . emission ;
lightingData . vertexLightingColor = 0 ;
lightingData . mainLightColor = 0 ;
lightingData . additionalLightsColor = 0 ;

return lightingData ;
}

half3 CalculateBlinnPhong ( Light light , InputData inputData , SurfaceData surfaceData )
{
half3 attenuatedLightColor = light . color * ( light . distanceAttenuation * light . shadowAttenuation ) ;
half3 lightDiffuseColor = LightingLambert ( attenuatedLightColor , light . direction , inputData . normalWS ) ;

half3 lightSpecularColor = half3 ( 0 , 0 , 0 ) ;
//#line 249
return lightDiffuseColor * surfaceData . albedo + lightSpecularColor ;

}
//#line 261
half4 UniversalFragmentPBR ( InputData inputData , SurfaceData surfaceData )
{
//#line 266
bool specularHighlightsOff = false ;

BRDFData brdfData ;
//#line 271
InitializeBRDFData ( surfaceData , brdfData ) ;
//#line 283
BRDFData brdfDataClearCoat = CreateClearCoatBRDFData ( surfaceData , brdfData ) ;
half4 shadowMask = CalculateShadowMask ( inputData ) ;
AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor ( inputData , surfaceData ) ;
uint meshRenderingLayers = GetMeshRenderingLayer ( ) ;
Light mainLight = GetMainLight ( inputData , shadowMask , aoFactor ) ;
//#line 290
MixRealtimeAndBakedGI ( mainLight , inputData . normalWS , inputData . bakedGI ) ;

LightingData lightingData = CreateLightingData ( inputData , surfaceData ) ;

lightingData . giColor = GlobalIllumination ( brdfData , brdfDataClearCoat , surfaceData . clearCoatMask ,
inputData . bakedGI , aoFactor . indirectAmbientOcclusion , inputData . positionWS ,
inputData . normalWS , inputData . viewDirectionWS , inputData . normalizedScreenSpaceUV ) ;

if ( IsMatchingLightLayer ( mainLight . layerMask , meshRenderingLayers ) )

{
lightingData . mainLightColor = LightingPhysicallyBased ( brdfData , brdfDataClearCoat ,
mainLight ,
inputData . normalWS , inputData . viewDirectionWS ,
surfaceData . clearCoatMask , specularHighlightsOff ) ;
}
//#line 308
uint pixelLightCount = GetAdditionalLightsCount ( ) ;
//#line 311
[ loop ] for ( uint lightIndex = 0 ; lightIndex < min ( ( ( uint ) _FPParams0 . w ) , ( 256 ) ) ; lightIndex ++ )
{
//#line 315
Light light = GetAdditionalLight ( lightIndex , inputData , shadowMask , aoFactor ) ;
//#line 318
if ( IsMatchingLightLayer ( light . layerMask , meshRenderingLayers ) )

{
lightingData . additionalLightsColor += LightingPhysicallyBased ( brdfData , brdfDataClearCoat , light ,
inputData . normalWS , inputData . viewDirectionWS ,
surfaceData . clearCoatMask , specularHighlightsOff ) ;
}
}
//#line 328
{ uint lightIndex ; ClusterIterator _urp_internal_clusterIterator = ClusterInit ( inputData . normalizedScreenSpaceUV , inputData . positionWS , 0 ) ; [ loop ] while ( ClusterNext ( _urp_internal_clusterIterator , lightIndex ) ) { lightIndex += ( ( uint ) _FPParams0 . w ) ;
Light light = GetAdditionalLight ( lightIndex , inputData , shadowMask , aoFactor ) ;
//#line 332
if ( IsMatchingLightLayer ( light . layerMask , meshRenderingLayers ) )

{
lightingData . additionalLightsColor += LightingPhysicallyBased ( brdfData , brdfDataClearCoat , light ,
inputData . normalWS , inputData . viewDirectionWS ,
surfaceData . clearCoatMask , specularHighlightsOff ) ;
}
} }
//#line 350
return CalculateFinalColor ( lightingData , surfaceData . alpha ) ;

}
//#line 355
half4 UniversalFragmentPBR ( InputData inputData , half3 albedo , half metallic , half3 specular ,
half smoothness , half occlusion , half3 emission , half alpha )
{
SurfaceData surfaceData ;

surfaceData . albedo = albedo ;
surfaceData . specular = specular ;
surfaceData . metallic = metallic ;
surfaceData . smoothness = smoothness ;
surfaceData . normalTS = half3 ( 0 , 0 , 1 ) ;
surfaceData . emission = emission ;
surfaceData . occlusion = occlusion ;
surfaceData . alpha = alpha ;
surfaceData . clearCoatMask = 0 ;
surfaceData . clearCoatSmoothness = 1 ;

return UniversalFragmentPBR ( inputData , surfaceData ) ;
}
//#line 377
half4 UniversalFragmentBlinnPhong ( InputData inputData , SurfaceData surfaceData )
{
//#line 388
uint meshRenderingLayers = GetMeshRenderingLayer ( ) ;
half4 shadowMask = CalculateShadowMask ( inputData ) ;
AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor ( inputData , surfaceData ) ;
Light mainLight = GetMainLight ( inputData , shadowMask , aoFactor ) ;

MixRealtimeAndBakedGI ( mainLight , inputData . normalWS , inputData . bakedGI , aoFactor ) ;

inputData . bakedGI *= surfaceData . albedo ;

LightingData lightingData = CreateLightingData ( inputData , surfaceData ) ;

if ( IsMatchingLightLayer ( mainLight . layerMask , meshRenderingLayers ) )

{
lightingData . mainLightColor += CalculateBlinnPhong ( mainLight , inputData , surfaceData ) ;
}
//#line 406
uint pixelLightCount = GetAdditionalLightsCount ( ) ;
//#line 409
[ loop ] for ( uint lightIndex = 0 ; lightIndex < min ( ( ( uint ) _FPParams0 . w ) , ( 256 ) ) ; lightIndex ++ )
{
//#line 413
Light light = GetAdditionalLight ( lightIndex , inputData , shadowMask , aoFactor ) ;

if ( IsMatchingLightLayer ( light . layerMask , meshRenderingLayers ) )

{
lightingData . additionalLightsColor += CalculateBlinnPhong ( light , inputData , surfaceData ) ;
}
}
//#line 423
{ uint lightIndex ; ClusterIterator _urp_internal_clusterIterator = ClusterInit ( inputData . normalizedScreenSpaceUV , inputData . positionWS , 0 ) ; [ loop ] while ( ClusterNext ( _urp_internal_clusterIterator , lightIndex ) ) { lightIndex += ( ( uint ) _FPParams0 . w ) ;
Light light = GetAdditionalLight ( lightIndex , inputData , shadowMask , aoFactor ) ;

if ( IsMatchingLightLayer ( light . layerMask , meshRenderingLayers ) )

{
lightingData . additionalLightsColor += CalculateBlinnPhong ( light , inputData , surfaceData ) ;
}
} }
//#line 438
return CalculateFinalColor ( lightingData , surfaceData . alpha ) ;
}
//#line 442
half4 UniversalFragmentBlinnPhong ( InputData inputData , half3 diffuse , half4 specularGloss , half smoothness , half3 emission , half alpha , half3 normalTS )
{
SurfaceData surfaceData ;

surfaceData . albedo = diffuse ;
surfaceData . alpha = alpha ;
surfaceData . emission = emission ;
surfaceData . metallic = 0 ;
surfaceData . occlusion = 1 ;
surfaceData . smoothness = smoothness ;
surfaceData . specular = specularGloss . rgb ;
surfaceData . clearCoatMask = 0 ;
surfaceData . clearCoatSmoothness = 1 ;
surfaceData . normalTS = normalTS ;

return UniversalFragmentBlinnPhong ( inputData , surfaceData ) ;
}
//#line 463
half4 UniversalFragmentBakedLit ( InputData inputData , SurfaceData surfaceData )
{
//#line 474
AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor ( inputData , surfaceData ) ;
LightingData lightingData = CreateLightingData ( inputData , surfaceData ) ;

if ( IsLightingFeatureEnabled ( ( 32 ) ) )
{
lightingData . giColor *= aoFactor . indirectAmbientOcclusion ;
}

return CalculateFinalColor ( lightingData , surfaceData . albedo , surfaceData . alpha , inputData . fogCoord ) ;
}
//#line 486
half4 UniversalFragmentBakedLit ( InputData inputData , half3 color , half alpha , half3 normalTS )
{
SurfaceData surfaceData ;

surfaceData . albedo = color ;
surfaceData . alpha = alpha ;
surfaceData . emission = half3 ( 0 , 0 , 0 ) ;
surfaceData . metallic = 0 ;
surfaceData . occlusion = 1 ;
surfaceData . smoothness = 1 ;
surfaceData . specular = half3 ( 0 , 0 , 0 ) ;
surfaceData . clearCoatMask = 0 ;
surfaceData . clearCoatSmoothness = 1 ;
surfaceData . normalTS = normalTS ;

return UniversalFragmentBakedLit ( inputData , surfaceData ) ;
}
//#line 19 "C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
struct Attributes
{
float4 positionOS : POSITION ;
float3 normalOS : NORMAL ;
float4 tangentOS : TANGENT ;
float2 texcoord : TEXCOORD0 ;
float2 staticLightmapUV : TEXCOORD1 ;
float2 dynamicLightmapUV : TEXCOORD2 ;

} ;

struct Varyings
{
float2 uv : TEXCOORD0 ;
//#line 35
float3 positionWS : TEXCOORD1 ;
//#line 38
float3 normalWS : TEXCOORD2 ;
//#line 46
half fogFactor : TEXCOORD5 ;
//#line 57
half3 vertexSH : TEXCOORD8 ;
//#line 62
float4 positionCS : SV_POSITION ;
//#line 65
} ;

void InitializeInputData ( Varyings input , half3 normalTS , out InputData inputData )
{
inputData = ( InputData ) 0 ;
//#line 72
inputData . positionWS = input . positionWS ;
//#line 79
half3 viewDirWS = GetWorldSpaceNormalizeViewDir ( input . positionWS ) ;
//#line 90
inputData . normalWS = input . normalWS ;
//#line 93
inputData . normalWS = NormalizeNormalPerPixel ( inputData . normalWS ) ;
inputData . viewDirectionWS = viewDirWS ;
//#line 99
inputData . shadowCoord = TransformWorldToShadowCoord ( inputData . positionWS ) ;
//#line 107
inputData . fogCoord = InitializeInputDataFog ( float4 ( input . positionWS , 1.0 ) , input . fogFactor ) ;
//#line 113
inputData . bakedGI = SampleProbeVolumePixel ( input . vertexSH , GetAbsolutePositionWS ( inputData . positionWS ) , inputData . normalWS , inputData . viewDirectionWS , input . positionCS . xy ) ;
//#line 118
inputData . normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV ( input . positionCS ) ;
inputData . shadowMask = unity_ProbesOcclusion ; ;
//#line 131
}
//#line 138
Varyings LitPassVertex ( Attributes input )
{
Varyings output = ( Varyings ) 0 ;

;
;
;

VertexPositionInputs vertexInput = GetVertexPositionInputs ( input . positionOS . xyz ) ;
//#line 151
VertexNormalInputs normalInput = GetVertexNormalInputs ( input . normalOS , input . tangentOS ) ;

half3 vertexLight = VertexLighting ( vertexInput . positionWS , normalInput . normalWS ) ;

half fogFactor = 0 ;
//#line 160
output . uv = ( ( input . texcoord . xy ) * _BaseMap_ST . xy + _BaseMap_ST . zw ) ;
//#line 163
output . normalWS = normalInput . normalWS ;
//#line 178
;
//#line 182
output . vertexSH . xyz = SampleProbeSHVertex ( vertexInput . positionWS , output . normalWS . xyz , GetWorldSpaceNormalizeViewDir ( vertexInput . positionWS ) ) ;
//#line 186
output . fogFactor = fogFactor ;
//#line 190
output . positionWS = vertexInput . positionWS ;
//#line 197
output . positionCS = vertexInput . positionCS ;

return output ;
}
//#line 203
void LitPassFragment (
Varyings input
, out half4 outColor : SV_Target0
//#line 209
)
{
;
;
//#line 224
SurfaceData surfaceData ;
InitializeStandardLitSurfaceData ( input . uv , surfaceData ) ;
//#line 231
InputData inputData ;
InitializeInputData ( input , surfaceData . normalTS , inputData ) ;
;
//#line 239
half4 color = UniversalFragmentPBR ( inputData , surfaceData ) ;
color . rgb = MixFog ( color . rgb , inputData . fogCoord ) ;
color . a = OutputAlpha ( color . a , IsSurfaceTypeTransparent ( _Surface ) ) ;

outColor = color ;
//#line 249
}