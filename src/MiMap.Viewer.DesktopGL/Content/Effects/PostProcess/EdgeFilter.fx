//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file EdgeFilter.fx
/// Draws silhouette outlines and crease edges using edge detection.
//
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Common.fxh
/// Frequently used constants and functions.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_COMMON_FXH
#define DIGITALRUNE_COMMON_FXH


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// The gamma value.
// Use 2.0 for approximate gamma (default) and 2.2 for exact gamma.
// See also comments in FromGamma/ToGamma().
#ifndef DR_GAMMA
#define DR_GAMMA 2.0
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The RGB weights to compute luminance with:
//    luminance = dot(color, LuminanceWeights);
// The color must be in linear space (not nonlinear sRGB values).
// Weights according to ITU Rec 601 (standard digital TV).
static const float3 LuminanceWeightsRec601 = float3(0.299, 0.587, 0.114);
// Weights according to ITU Rec 709 (HDTV). Same as sRGB.
static const float3 LuminanceWeightsRec709 = float3(0.2126, 0.7152, 0.0722);
static const float3 LuminanceWeights = LuminanceWeightsRec709;

static const float Pi = 3.1415926535897932384626433832795;

// A bias matrix that converts a projection space vector into texture space.
// (x, y) coordinates in projection space range from (-1, -1) at the bottom left
// to (1, 1) at the top right. For texturing the top left should be (0, 0) and
// the bottom right should be (1, 1).
static const float4x4 ProjectorBiasMatrix = { 0.5,    0, 0, 0,
                                                0, -0.5, 0, 0,
                                                0,    0, 1, 0,
                                              0.5,  0.5, 0, 1 };


/// Declares the uniform const for a jitter map texture + sampler.
/// \param[in] name   The name of the jitter map texture constant.
/// \remarks
/// Example: To declare JitterMap and JitterMapSampler call
///   DECLARE_UNIFORM_JITTERMAP(JitterMap);
#define DECLARE_UNIFORM_JITTERMAP(name) \
Texture2D name : JITTERMAP; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = WRAP; \
  AddressV  = WRAP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = NONE; \
}


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Converts a color component given in non-linear gamma space to linear space.
/// \param[in] colorGamma  The color component in non-linear gamma space.
/// \return The color component in linear space.
float FromGamma(float colorGamma)
{
  // If DR_GAMMA is not 2, we might have to use pow(0.000001 + ..., ...).
  return pow(colorGamma, DR_GAMMA);
}


/// Converts a color given in non-linear gamma space to linear space.
/// \param[in] colorGamma  The color in non-linear gamma space.
/// \return The color in linear space.
float3 FromGamma(float3 colorGamma)
{
  // If DR_GAMMA is not 2, we might have to use pow(0.000001 + ..., ...).
  return pow(colorGamma, DR_GAMMA);
}


/// Converts a color component given in linear space to non-linear gamma space.
/// \param[in] colorLinear  The color component in linear space.
/// \return The color component in non-linear gamma space.
float ToGamma(float colorLinear)
{
  // If DR_GAMMA is not 2, we might have to use pow(0.000001 + ..., ...).
  return pow(colorLinear, 1 / DR_GAMMA);
}


/// Converts a color given in linear space to non-linear gamma space.
/// \param[in] colorLinear  The color in linear space.
/// \return The color in non-linear gamma space.
float3 ToGamma(float3 colorLinear)
{
  // If DR_GAMMA is not 2, we might have to use pow(0.000001 + ..., ...).
  return pow(colorLinear, 1 / DR_GAMMA);
}


/// Transforms a screen space position to standard projection space.
/// The half pixel offset for correct texture alignment is applied.
/// (Note: This half pixel offset is only necessary in DirectX 9.)
/// \param[in] position     The screen space position in "pixels".
/// \param[in] viewportSize The viewport size in pixels, e.g. (1280, 720).
/// \return The position in projection space.
float2 ScreenToProjection(float2 position, float2 viewportSize)
{
#if !SM4
  // Subtract a half pixel so that the edge of the primitive is between screen pixels.
  // Thus, the first texel lies exactly on the first pixel.
  // See also http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
  // for a good description of this DirectX 9 problem.
  position -= 0.5;
#endif
  
  // Now transform screen space coordinate into projection space.
  // Screen space: Left top = (0, 0), right bottom = (ScreenSize.x - 1, ScreenSize.y - 1).
  // Projection space: Left top = (-1, 1), right bottom = (1, -1).
  
  // Transform into the range [0, 1] x [0, 1].
  position /= viewportSize;
  // Transform into the range [0, 2] x [-2, 0]
  position *= float2(2, -2);
  // Transform into the range [-1, 1] x [1, -1].
  position -= float2(1, -1);
  
  return position;
}


/// Transforms a screen space position to standard projection space.
/// The half pixel offset for correct texture alignment is applied.
/// (Note: This half pixel offset is only necessary in DirectX 9.)
/// \param[in] position     The screen space position in "pixels".
/// \param[in] viewportSize The viewport size in pixels, e.g. (1280, 720).
/// \return The position in projection space.
float4 ScreenToProjection(float4 position, float2 viewportSize)
{
  position.xy = ScreenToProjection(position.xy, viewportSize);
  return position;
}


/// Transforms a position from standard projection space to screen space.
/// The half pixel offset for correct texture alignment is applied.
/// (Note: This half pixel offset is only necessary in DirectX 9.)
/// \param[in] position     The position in projection space.
/// \param[in] viewportSize The viewport size in pixels, e.g. (1280, 720).
/// \return The screen space position in texture coordinates ([0, 1] range).
float2 ProjectionToScreen(float4 position, float2 viewportSize)
{
  // Perspective divide:
  position.xy = position.xy / position.w;
  
  // Convert from range [-1, 1] x [1, -1] to [0, 1] x [0, 1].
  position.xy = float2(0.5, -0.5) * position.xy + float2(0.5, 0.5);
  
  // The position (0, 0) is the center of the first screen pixel. We have
  // to add half a texel to sample the center of the first texel.
#if !SM4
  position.xy += 0.5f / viewportSize;
#endif
  
  return position.xy;
}


/// Converts a clip space depth (normal depth buffer value) back to view space.
/// \param[in] depth  The clip space depth value.
/// \param[in] near   The distance of the camera near plane.
/// \param[in] far    The distance of the camera far plane.
/// \return The depth in view space in the range [near, far].
/// \remarks
/// The returned depth is positive and describes the planar distance from the
/// camera (if it were the z coordinate it would be negative).
/// (Note: This function only works for perspective projections.)
float ConvertDepthFromClipSpaceToViewSpace(float depth, float near, float far)
{
  // This operation inverts the perspective projection matrix and the
  // perspective divide.
  float q = far / (near - far);
  return near * q / (depth + q);
}


//-----------------------------------------------------------------------------
// Texture Mapping
//-----------------------------------------------------------------------------

/// Computes the current mip map level for a texture.
/// \param[in] texCoord      The texture coordinates.
/// \param[in] textureSize   The size of the texture in pixels (width, height).
/// \return The mip map level.
float MipLevel(float2 texCoord, float2 textureSize)
{
  // Note: This code is taken from Shader X5 - Practical Parallax Occlusion Mapping
  // which is similar to the DirectX 2009 Parallax Occlusion Mapping sample code.
  
  // Compute mip map level for texture.
  float2 scaledTexCoord = texCoord * textureSize;
  
  // Compute partial derivatives of the texture coordinates with respect to screen
  // coordinates. The derivatives are an approximation of the pixel's size in texture
  // space.
  float2 dxSize = ddx(scaledTexCoord);
  float2 dySize = ddy(scaledTexCoord);
  
  // Find max of change in u and v across quad: Compute du and dv magnitude across quad.
  float2 dTexCoord = dxSize * dxSize + dySize * dySize;
  float maxTexCoordDelta = max(dTexCoord.x, dTexCoord.y);
  
  // The mip map level k for a given compression value d is such that
  //   2^k <= d < 2^(k+1)
  // Or:
  //   k = floor(log2(d))
  
  // Compute the current mip map level.
  // (The factor 0.5 is effectively computing a square root before log.)
  return max(0.5 * log2(maxTexCoordDelta), 0);
}


/// Computes the current mip map level for a texture and anisotropic filtering.
/// \param[in] texCoord       The texture coordinates.
/// \param[in] textureSize    The size of the texture in pixels (width, height).
/// \param[in] maxAnisotropy  The max anistropy, e.g. 16 for 16xAniso.
/// \return The mip map level.
float MipLevel(float2 texCoord, float2 textureSize, float maxAnisotropy)
{
  // Note: This code is taken from Shader X5 - Practical Parallax Occlusion Mapping
  // which is similar to the DirectX 2009 Parallax Occlusion Mapping sample code.
  
  // Compute mip map level for texture.
  float2 scaledTexCoord = texCoord * textureSize;
  
  // Compute partial derivatives of the texture coordinates with respect to screen
  // coordinates. The derivatives are an approximation of the pixel's size in texture
  // space.
  float2 dxSize = ddx(scaledTexCoord);
  float2 dySize = ddy(scaledTexCoord);
  
  // According to OpenGL specs:
  ////vec2 ddx=dFdx(gl_TexCoord[0].xy);
  ////vec2 ddy=dFdy(gl_TexCoord[0].xy);
  ////float Px = length(ddx);
  ////float Py = length(ddy);
  float Px = length(dxSize);
  float Py = length(dySize);
  float Pmax = max(Px, Py);
  float Pmin = min(Px, Py);
  //float N = min(ceil(Pmax/Pmin), maxAnisotropy);   // OpenGL spec
  float N = min(Pmax/Pmin, maxAnisotropy);     // This actually works and makes more sense.
  float lambda = max(0, log2(Pmax / N));
  return lambda;
}


// Samples the given texture using manual bilinear filtering.
/// \param[in] textureSampler  The texture sampler (which uses POINT filtering).
/// \param[in] texCoord        The texture coordinates.
/// \param[in] textureSize     The size of the texture in pixels (width, height).
/// \return The texture sample.
float4 SampleLinear(sampler textureSampler, float2 texCoord, float2 textureSize)
{
  float2 texelSize = 1.0 / textureSize;
  texCoord -= 0.5 * texelSize;
  float4 s00 = tex2D(textureSampler, texCoord);
  float4 s10 = tex2D(textureSampler, texCoord + float2(texelSize.x, 0));
  float4 s01 = tex2D(textureSampler, texCoord + float2(0, texelSize.y));
  float4 s11 = tex2D(textureSampler, texCoord + texelSize);
  
  float2 texelpos = textureSize * texCoord;
  float2 lerps = frac(texelpos);
  return lerp(lerp(s00, s10, lerps.x), lerp(s01, s11, lerps.x), lerps.y);
}


// Samples the given texture using manual bilinear filtering.
/// \param[in] textureSampler  The texture sampler (which uses POINT filtering).
/// \param[in] texCoord        The texture coordinates for tex2Dlod.
/// \param[in] textureSize     The size of the texture in pixels (width, height).
/// \return The texture sample.
float4 SampleLinearLod(sampler textureSampler, float4 texCoord, float2 textureSize)
{
  // Texel size relative to the mip level:
  float2 texelSize = 1.0 / textureSize * exp2(texCoord.w);
  texCoord.xy -= 0.5 * texelSize;
  float4 s00 = tex2Dlod(textureSampler, float4(texCoord.xy, texCoord.z, texCoord.w));
  float4 s10 = tex2Dlod(textureSampler, float4(texCoord.xy + float2(texelSize.x, 0), texCoord.z, texCoord.w));
  float4 s01 = tex2Dlod(textureSampler, float4(texCoord.xy + float2(0, texelSize.y), texCoord.z, texCoord.w));
  float4 s11 = tex2Dlod(textureSampler, float4(texCoord.xy + texelSize, texCoord.z, texCoord.w));
  
  float2 texelpos = textureSize * texCoord.xy;
  float2 lerps = frac(texelpos);
  return lerp(lerp(s00, s10, lerps.x), lerp(s01, s11, lerps.x), lerps.y);
}

// Samples the given texture using manual trilinear filtering. (Not fully tested!!!)
/// \param[in] textureSampler  The texture sampler (which uses POINT filtering).
/// \param[in] texCoord        The texture coordinates for tex2Dlod.
/// \param[in] textureSize     The size of the texture in pixels (width, height).
/// \return The texture sample.
float4 SampleTrilinear(sampler textureSampler, float2 texCoord, float2 textureSize)
{
  float mipLevel = MipLevel(texCoord, textureSize);
  float minMipLevel = (int)mipLevel;
  float maxMipLevel = minMipLevel + 1;
  float lerpParameter = frac(mipLevel);
  
  return lerp(SampleLinearLod(textureSampler, float4(texCoord.xy, 0, minMipLevel), textureSize),
              SampleLinearLod(textureSampler, float4(texCoord.xy, 0, maxMipLevel), textureSize),
              lerpParameter);
}


/// Helps to visualize the resolution of a texture for debugging. This function
/// can be used to draw grid lines between the texels or to draw a checkerboard
/// pattern.
/// \param[in] texCoord    The current texture coordinate.
/// \param[in] textureSize The size of the texture as float2(width, height).
/// \param[in] gridSize    The relative size of the grid lines in percent, for
///                        instance 0.02. If this value is 0 or negative a
///                        checkerboard pattern is created instead of a grid.
///  \return  0 if a dark pixel should be drawn for this texture coordinate;
///           otherwise 1.
float VisualizeTextureGrid(float2 texCoord, float2 textureSize, float gridSize)
{
  if (gridSize > 0)
  {
    float2 t = frac(texCoord * textureSize);
    return (t.x < gridSize || t.x > 1 - gridSize)
           || (t.y < gridSize || t.y > 1 - gridSize);
  }
  else
  {
    int2 t = int2(texCoord * textureSize);
    return (t.x % 2 == 0 && t.y % 2 == 0)
           || (t.x % 2 == 1 && t.y % 2 == 1);
  }
}


//-----------------------------------------------------------------------------
// Normal Mapping
//-----------------------------------------------------------------------------

/// Gets the normal vector from a standard normal texture (no special encoding).
/// \param[in] normalSampler The sampler for the normal texture.
/// \param[in] texCoord      The texture coordinates.
/// \return The normalized normal.
float3 GetNormal(sampler normalSampler, float2 texCoord)
{
  float3 normal = tex2D(normalSampler, texCoord).xyz * 255/128 - 1;
  normal = normalize(normal);
  return normal;
}


/// Gets the normal vector from a normal texture which uses DXT5nm encoding.
/// \param[in] normalSampler The sampler for the normal texture.
/// \param[in] texCoord      The texture coordinates.
/// \return The normalized normal.
float3 GetNormalDxt5nm(sampler normalSampler, float2 texCoord)
{
  float3 normal;
  normal.xy = tex2D(normalSampler, texCoord).ag * 255/128 - 1;
  normal.z = sqrt(1.0 - dot(normal.xy, normal.xy));
  return normal;
}


/// Gets the normal vector from a normal texture which uses DXT5nm encoding.
/// \param[in] normalSampler The sampler for the normal texture.
/// \param[in] texCoord      The texture coordinates.
/// \param[in] ddxTexCoord   ddx(texCoord)
/// \param[in] ddyTexCoord   ddx(texCoord)
/// \return The normalized normal.
float3 GetNormalDxt5nm(sampler normalSampler, float2 texCoord, float2 ddxTexCoord, float2 ddyTexCoord)
{
  float3 normal;
  normal.xy = tex2Dgrad(normalSampler, texCoord, ddxTexCoord, ddyTexCoord).ag * 255/128 - 1;
  normal.z = sqrt(1.0 - dot(normal.xy, normal.xy));
  return normal;
}


/// Computes the cotangent frame.
/// \param[in] n   The (normalized) normal vector.
/// \param[in] p   The position.
/// \param[in] uv  The texture coordinates.
/// \return The cotangent frame.
/// \remarks
/// For reference see http://www.thetenthplanet.de/archives/1180.
/// Example: To convert a normal n from a normal map to world space.
///  float3x3 cotangentFrame = CotangentFrame(normalWorld, positionWorld, texCoord);
///  n.y = -n.y;   // Invert y for our standard "green-up" normal maps.
///  nWorld = mul(n, cotangentFrame);
float3x3 CotangentFrame(float3 n, float3 p, float2 uv)
{
  // Get edge vectors of the pixel triangle.
  float3 dp1 = ddx(p);
  float3 dp2 = ddy(p);
  float2 duv1 = ddx(uv);
  float2 duv2 = ddy(uv);
  
  // ----- Original
  // Solve the linear system.
  //float3x3 M = float3x3(dp1, dp2, cross(dp1, dp2));
  //float3x3 inverseM = Invert(M);
  //float3 T = mul(inverseM, float3(duv1.x, duv2.x, 0));
  //float3 B = mul(inverseM, float3(duv1.y, duv2.y, 0));
  
  // ----- Optimized
  float3 dp2perp = cross(n, dp2);
  float3 dp1perp = cross(dp1, n);
  float3 t = dp2perp * duv1.x + dp1perp * duv2.x;
  float3 b = dp2perp * duv1.y + dp1perp * duv2.y;
  
  // Construct a scale-invariant frame.
  float invmax = rsqrt(max(dot(t, t), dot(b, b)));
  return float3x3(t * invmax, b * invmax, n);
}


//-----------------------------------------------------------------------------
// Util
//-----------------------------------------------------------------------------

/// Creates a right-handed, look-at matrix.
/// \param[in] eyePosition     Eye position (position of the viewer).
/// \param[in] targetPosition  The point where the viewer is looking at.
/// \param[in] upVector        The up-vector of the viewer.
/// \return The look-at matrix.
float4x4 CreateLookAt(float3 eyePosition, float3 targetPosition, float3 upVector)
{
  float3 zAxis = normalize(eyePosition - targetPosition);
  float3 xAxis = normalize(cross(upVector, zAxis));
  float3 yAxis = cross(zAxis, xAxis);
  
  float4x4 view =
  {
    xAxis.x, yAxis.x, zAxis.x, 0,
    xAxis.y, yAxis.y, zAxis.y, 0,
    xAxis.z, yAxis.z, zAxis.z, 0,
    -dot(xAxis, eyePosition), -dot(yAxis, eyePosition), -dot(zAxis, eyePosition), 1
  };
  
  return view;
}


/// Computes an orthnormal base for the given vector.
/// \param[in] n        A normalized vector.
/// \param[out] b1      A normalized vector orthogonal to n.
/// \param[out] b2      A normalized vector orthogonal to n and b1.
void GetOrthonormals(float3 n, out float3 b1, out float3 b2)
{
  // This method was presented in
  // "Building an Orthonormal Basis from a 3D Unit Vector Without Normalization"
  // http://orbit.dtu.dk/fedora/objects/orbit:113874/datastreams/file_75b66578-222e-4c7d-abdf-f7e255100209/content
  
  if(n.z < -0.9999999) // Handle the singularity.
  {
    b1 = float3(0, -1, 0);
    b2 = float3(-1, 0, 0);
    return;
  }
  
  float a = 1 / (1 + n.z);
  float b = -n.x * n.y * a ;
  b1 = float3(1 - n.x * n.x * a, b, -n.x);
  b2 = float3(b , 1 - n.y * n.y * a, -n.y);
}


/// Computes where a ray hits a sphere (which is centered at the origin).
/// \param[in]  rayOrigin    The start position of the ray.
/// \param[in]  rayDirection The normalized direction of the ray.
/// \param[in]  radius       The radius of the sphere.
/// \param[out] enter        The ray parameter where the ray enters the sphere.
///                          0 if the ray is already in the sphere.
/// \param[out] exit         The ray parameter where the ray exits the sphere.
/// \return  0 or a positive value if the ray hits the sphere. A negative value
///          if the ray does not touch the sphere.
float HitSphere(float3 rayOrigin, float3 rayDirection, float radius, out float enter, out float exit)
{
  // Solve the equation:  ||rayOrigin + distance * rayDirection|| = r
  //
  // This is a straight-forward quadratic equation:
  //   ||O + d * D|| = r
  //   =>  (O + d * D)² = r²  where V² means V.V
  //   =>  d² * D² + 2 * d * (O.D) + O² - r² = 0
  // D² is 1 because the rayDirection is normalized.
  //   =>  d = -O.D + sqrt((O.D)² - O² + r²)
  
  float OD = dot(rayOrigin, rayDirection);
  float OO = dot(rayOrigin, rayOrigin);
  float radicand = OD * OD - OO + radius * radius;
  enter = max(0, -OD - sqrt(radicand));
  exit = -OD + sqrt(radicand);
  
  return radicand;  // If radicand is negative then we do not have a result - no hit.
}


/// Inverts the specified 3x3 matrix.
/// \param[in] m     The 3x3 matrix to be inverted.
/// \return The inverse of the matrix.
float3x3 Invert(float3x3 m)
{
  float det = determinant(m); // = dot(cross(m[0], m[1]), m[2]);
  float3x3 t = transpose(m);
  float3x3 adjugate = float3x3(cross(t[1], t[2]),
                               cross(t[2], t[0]),
                               cross(t[0], t[1]));
  return adjugate / det;
}


/// Checks if the given vector elements are all in the range [min, max].
/// \param[in] x    The vector that should be checked.
/// \param[in] min  The minimal allowed range.
/// \param[in] max  The maximal allowed range.
/// \return True if all elements of x are in the range [min, max].
bool IsInRange(float3 x, float min, float max)
{
  return all(clamp(x, min, max) == x);
}


/// Checks if the given vector elements are all in the range [min, max].
/// \param[in] x    The vector that should be checked.
/// \param[in] min  The minimal allowed range.
/// \param[in] max  The maximal allowed range.
/// \return True if all elements of x are in the range [min, max].
bool IsInRange(float4 x, float min, float max)
{
  return all(clamp(x, min, max) == x);
}


/// Returns a linear interpolation betwenn 0 and 1 if x is in the range [min, max].
/// This does the same as the HLSL intrinsic function smoothstep() - but without
/// a smooth curve.
///  min  The minimum range of the x parameter.
///  max  The maximum range of the x parameter.
///  x    The specified value to be interpolated.
///  Returns 0 if x is less than min;
///  1 if x is greater than max;
///  otherwise, a value between 0 and 1 if x is in the range [min, max].
float LinearStep(float min, float max, float x)
{
  float y = (x - min) / (max - min);
  return clamp(y, 0, 1);
}


/// Calculates the logarithm for a given y and base, such that base^x = y.
/// param[in] base    The base of the logarithm.
/// param[in] y       The number of which to calculate the logarithm.
/// \return The logarithm of y.
float Log(float base, float y)
{
  return log(y) / log(base);
}
#endif
//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Encoding.fxh
/// Functions to encode/decode values.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_ENCODING_FXH
#define DIGITALRUNE_ENCODING_FXH

// Notes:
// (An alternative to RGBM would be RGBD (http://iwasbeingirony.blogspot.com/2010_06_01_archive.html).
// RGBM better conserves the full range and is better in most cases. RGBD conserves the low lights
// better and can be used if you only need occasional highlights.)


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// Matrix for encoding RGB to LogLuv.
static const float3x3 LogLuvMatrix = float3x3(0.2209, 0.3390, 0.4184,
                                              0.1138, 0.6780, 0.7319,
                                              0.0102, 0.1130, 0.2969);
// Inverse matrix for decoding LogLuv to RGB.
static const float3x3 LogLuvMatrixInverse = float3x3(6.0013, -2.700,  -1.7995,
                                                    -1.332,   3.1029, -5.7720,
                                                     0.3007, -1.088,   5.6268);


/// Declares the uniform const for the normals fitting texture + sampler.
/// \remarks
/// The normals fitting textures is a lookup texture used to encode normals in
/// EncodeNormalBestFit().
#define DECLARE_UNIFORM_NORMALSFITTINGTEXTURE \
texture NormalsFittingTexture : NORMALSFITTINGTEXTURE; \
sampler NormalsFittingSampler = sampler_state \
{ \
  Texture = <NormalsFittingTexture>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = POINT; \
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

// ---- Accurate, but slow RGBE encoding
// Reference: Wolfgang Engel, "Programming Vertex and Pixel Shader", pp. 230

// Constants for RGBE encoding.
static const float RgbeBase = 1.04;
static const float RgbeOffset = 64.0;


/// Calculates the logarithm for a given y and base, such that base^x = y.
/// param[in] base    The base of the logarithm.
/// param[in] y       The number of whic to calculate the logarithm.
/// \return The logarithm of y.
float LogEnc(float base, float y)
{
  // We use an "obsfuscated" name because the same method is declared in
  // Misc.fxh but we don't want to include that or create a duplicate definition.
  return log(y) / log(base);
}


/// Encodes the given color to RGBE 8-bit format.
/// \param[in] color    The original color.
/// \return The color encoded as RGBE.
float4 EncodeRgbe_Engel(float3 color)
{
  // Get the largest component.
  float maxValue = max(max(color.r, color.g), color.b);
  
  float exponent = floor(LogEnc(RgbeBase, maxValue));
  
  float4 result;
  
  // Store the exponent in the alpha channel.
  result.a = clamp((exponent + RgbeOffset) / 255, 0.0, 1.0 );
  
  // Convert the color channels.
  result.rgb = color / pow(RgbeBase, result.a * 255 - RgbeOffset);
  
  return result;
}


/// Decodes the given color from RGBE 8-bit format.
/// \param[in] rgbe   The color encoded as RGBE.
/// \return The orginal color.
float3 DecodeRgbe_Engel(float4 rgbe)
{
  // Get exponent from alpha channel.
  float exponent = rgbe.a * 255 - RgbeOffset;
  float scale = pow(RgbeBase, exponent);
  
  return rgbe.rgb * scale;
}
// -----


/// Encodes the given color to RGBE 8-bit format.
/// \param[in] color    The original color.
/// \return The color encoded as RGBE.
float4 EncodeRgbe(float3 color)
{
  // Get the largest component.
  float maxValue = max(max(color.r, color.g), color.b);
  
  float exponent = ceil(log2(maxValue));
  
  float4 result;
  
  // Store the exponent in the alpha channel.
  result.a = (exponent + 128) / 255;
  
  // Convert the color channels.
  result.rgb = color / exp2(exponent);
  
  return result;
}


/// Decodes the given color from RGBE 8-bit format.
/// \param[in] rgbe   The color encoded as RGBE.
/// \return The orginal color.
float3 DecodeRgbe(float4 rgbe)
{
  // Get exponent from alpha channel.
  float exponent = rgbe.a * 255 - 128;
  
  return rgbe.rgb * exp2(exponent);
}


/// Encodes the given float value in the range [0, 1] in a RGBA format (4 x 8 bit).
/// \param[in] value    The original value.
/// \return The value encoded as RGBA.
/// \remarks
/// The result is undefined if value is <0 or >1.
float4 EncodeFloatInRgba(float value)
{
  float4 result = value * float4(1,
                                 255.0,
                                 255.0 * 255.0,
                                 255.0 * 255.0 * 255.0);
  result.yzw = frac(result.yzw);
  result -= result.yzww * float4(1.0 / 255.0,
                                 1.0 / 255.0,
                                 1.0 / 255.0,
                                 0.0);
  return result;
}


/// Decodes the float value that was stored in a RGBA format (4 x 8 bit).
/// \param[in] rgba    The value encoded as RGBA.
/// \return The original value.
float DecodeFloatFromRgba(float4 rgba)
{
  float4 factors = float4(1.0,
                          1.0 / 255.0,
                          1.0 / (255.0 * 255.0),
                          1.0 / (255.0 * 255.0 * 255.0));
  return dot(rgba, factors);
}


/// Encodes the given color to LogLuv format.
/// \param[in] color    The original color.
/// \return The color encoded as LogLuv.
float4 EncodeLogLuv(float3 color)
{
  // See http://xnainfo.com/content.php?content=17,
  //     http://realtimecollisiondetection.net/blog/?p=15.
  
  float3 Xp_Y_XYZp = mul(color, LogLuvMatrix);
  Xp_Y_XYZp = max(Xp_Y_XYZp, float3(1e-6, 1e-6, 1e-6));   // Avoid values <= 0.
  float4 result;
  result.xy = Xp_Y_XYZp.xy / Xp_Y_XYZp.z;
  float Le = 2 * log2(Xp_Y_XYZp.y) + 127;
  result.w = frac(Le);
  result.z = (Le - (floor(result.w*255.0f))/255.0f)/255.0f;
  return result;
}


/// Decodes the given color from LogLuv format.
/// \param[in] logLuv   The color encoded as LogLuv.
/// \return The orginal color.
float3 DecodeLogLuv(float4 logLuv)
{
  float Le = logLuv.z * 255 + logLuv.w;
  float3 Xp_Y_XYZp;
  Xp_Y_XYZp.y = exp2((Le - 127) / 2);
  Xp_Y_XYZp.z = Xp_Y_XYZp.y / logLuv.y;
  Xp_Y_XYZp.x = logLuv.x * Xp_Y_XYZp.z;
  float3 vRGB = mul(Xp_Y_XYZp, LogLuvMatrixInverse);
  return max(vRGB, 0);
}


/// Encodes the given color to RGBM format.
/// \param[in] color    The original color.
/// \param[in] maxValue The max value, e.g. 6 (if color is gamma corrected) =
///                     6 ^ 2.2 (if color is in linear space).
/// \return The color in RGBM format.
/// \remarks
/// The input color can be in linear space or in gamma space. It is recommended
/// convert the color to gamma space before encoding as RGBM.
/// See http://graphicrants.blogspot.com/2009/04/rgbm-color-encoding.html.
float4 EncodeRgbm(float3 color, float maxValue)
{
  float4 rgbm;
  color /= maxValue;
  rgbm.a = saturate(max(max(color.r, color.g), max(color.b, 1e-6)));
  rgbm.a = ceil(rgbm.a * 255.0) / 255.0;
  rgbm.rgb = color / rgbm.a;
  return rgbm;
}


/// Decodes the given color from RGBM format.
/// \param[in] rgbm      The color in RGBM format.
/// \param[in] maxValue  The max value, e.g. 6 (if color is gamma corrected) =
///                      6 ^ 2.2 (if color is in linear space).
/// \return The original RGB color (can be in linear or gamma space).
float3 DecodeRgbm(float4 rgbm, float maxValue)
{
  return maxValue * rgbm.rgb * rgbm.a;
}


/// Encodes a normal vector in 3 8-bit channels.
/// \param[in] normal                 The normal vector.
/// \param[in] normalsFittingSampler  The lookup texture for normal fitting.
/// \return The normal encoded for storage in an RGB texture (3 x 8 bit).
half3 EncodeNormalBestFit(half3 normal, sampler normalsFittingSampler)
{
  // Best-fit normal encoding as in "CryENGINE 3: Reaching the Speed of Light"
  // by Anton Kaplanyan (Crytek). See http://advances.realtimerendering.com/s2010/index.html
  
  // Renormalize (needed if any blending or interpolation happened before).
  normal.rgb = (half3)normalize(normal.rgb);
  // Get unsigned normal for cubemap lookup. (Note, the full float precision is required.)
  half3 unsignedNormal = abs(normal.rgb);
  // Get the main axis for cubemap lookup.
  half maxNAbs = max(unsignedNormal.z, max(unsignedNormal.x, unsignedNormal.y));
  // Get texture coordinates in a collapsed cubemap.
  float2 texcoord = unsignedNormal.z < maxNAbs ? (unsignedNormal.y < maxNAbs ? unsignedNormal.yz : unsignedNormal.xz) : unsignedNormal.xy;
  texcoord = texcoord.x < texcoord.y ? texcoord.yx : texcoord.xy;
  texcoord.y /= texcoord.x;
  // Fit normal into the edge of unit cube.
  normal.rgb /= maxNAbs;
  // Look-up fitting length and scale the normal to get the best fit.
  half fittingScale = (half)tex2D(normalsFittingSampler, texcoord).a;
  // Scale the normal to get the best fit.
  normal.rgb *= fittingScale;
  // Squeeze back to unsigned.
  normal.rgb = normal.rgb * 0.5h + 0.5h;
  return normal;
}


/// Decodes a normal that was encoded with EncodeNormalBestFit().
/// \param[in] encodedNormal    The encoded normal.
/// \return The original normal.
half3 DecodeNormalBestFit(half4 encodedNormal)
{
  return (half3)normalize(encodedNormal.xyz * 2 - 1);
}


/// Encodes a normal vector in 2 channels (with 8 bit or more per channel).
/// \param[in] normal   The normal vector.
/// \return The normal encoded for storage in 2 channels.
half2 EncodeNormalSphereMap(half3 normal)
{
  // See http://aras-p.info/texts/CompactNormalStorage.html.
  half2 encodedNormal = (half2)normalize(normal.xy) * (half)sqrt(-normal.z*0.5+0.5);
  encodedNormal = encodedNormal * 0.5 + 0.5;
  return encodedNormal;
}


/// Decodes a normal that was encoded with EncodeNormalSphereMap().
/// \param[in] encodedNormal    The encoded normal.
/// \return The original normal.
half3 DecodeNormalSphereMap(half4 encodedNormal)
{
  half4 nn = encodedNormal * half4(2, 2, 0, 0) + half4(-1, -1, 1, -1);
  half l = dot(nn.xyz, -nn.xyw);
  nn.z = l;
  nn.xy *= (half)sqrt(l);
  return nn.xyz * 2 + half3(0, 0, -1);
}


/// Encodes a normal vector in 2 channels (with 8 bit or more per channel).
/// \param[in] normal   The normal vector.
/// \return The normal encoded for storage in 2 channels.
half2 EncodeNormalStereographic(half3 normal)
{
  // See http://aras-p.info/texts/CompactNormalStorage.html.
  half scale = 1.7777;
  half2 result = normal.xy / (normal.z + 1);
  result /= scale;
  result = result * 0.5 + 0.5;
  return result;
}


/// Decodes a normal that was encoded with EncodeNormalStereographic().
/// \param[in] encodedNormal    The encoded normal.
/// \return The original normal.
half3 DecodeNormalStereographic(half4 encodedNormal)
{
  half scale = 1.7777;
  half3 nn = encodedNormal.xyz * half3(2 * scale, 2 * scale, 0) + half3(-scale, -scale, 1);
  half g = 2.0 / dot(nn.xyz, nn.xyz);
  half3 normal;
  normal.xy = g * nn.xy;
  normal.z = g - 1;
  return normal;
}


/// Encodes a specular power (to be stored as unsigned byte).
/// \param[in] specularPower   The specular power.
/// \return The encoded specular power [0, 1].
float EncodeSpecularPower(float specularPower)
{
  // Linear packing:
  //   Compress range [0, max] --> [0, 1].
  //   y = x / max
  //return specularPower / 100.0f;
  // Or:
  //return specularPower / 512.0f;
  
  // Logarithmic packing (similar to Killzone):
  //   Compress range [1, max] --> [0, 1].
  //   y = log2(x) / log2(max)
  return log2(specularPower + 0.0001f) / 17.6f; // max = 200000
  
  // Unreal Engine Elemental Demo
  //return (log2(specularPower + 0.0001f) + 1) / 19.0f;
}


/// Decodes the specular power (stored as unsigned byte).
/// \param[in] encodedSpecularPower    The encoded specular power [0, 1].
/// \return The original specular power.
float DecodeSpecularPower(float encodedSpecularPower)
{
  //return encodedSpecularPower * 100.0f;
  //return encodedSpecularPower * 512.0f;
  return exp2(encodedSpecularPower * 17.6f);
  //return exp2(encodedSpecularPower * 19.0f - 1);
}
#endif
//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Deferred.fxh
/// Functions for deferred rendering (e.g. G-buffer and light buffer access).
/// 
/// If you are creating the G-buffer, you need to define
///   #define CREATE_GBUFFER 1
/// before including Deferred.fxh.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_DEFERRED_FXH
#define DIGITALRUNE_DEFERRED_FXH

#ifndef DIGITALRUNE_ENCODING_FXH
#error "Encoding.fxh required. Please include Encoding.fxh before including Deferred.fxh."
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

#if CREATE_GBUFFER
DECLARE_UNIFORM_NORMALSFITTINGTEXTURE
#endif


/// Declares the uniform const for the view frustum far corners in view space.
/// \param[in] name   The name of the constant.
/// \remarks
/// Order of the corners: (top left, top right, bottom left, bottom right)
/// Usually you will call
///   DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);
#define DECLARE_UNIFORM_FRUSTUMCORNERS(name) float3 name[4]


/// Declares the uniform const for the view frustum info for reconstructing the
/// view space position from texture coordinates.
/// \param[in] name   The name of the constant.
/// \remarks
/// The const values are:
/// (Left / Near, Top / Near, (Right - Left) / Near, (Bottom - Top) / Near)
#define DECLARE_UNIFORM_FRUSTUMINFO(name) float4 name


/// Declares the uniform const for a G-buffer texture + sampler.
/// \param[in] name   The name of the texture constant.
/// \param[in] index  The index of the G-buffer.
/// \remarks
/// Example: To declare GBuffer0 and GBuffer0Sampler call
///   DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
/// Usually you will use
///  DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
///  DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);
#define DECLARE_UNIFORM_GBUFFER(name, index) \
texture name : GBUFFER##index; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = NONE; \
}


/// Declares the uniform const for a light buffer texture + sampler.
/// \param[in] name   The name of the light buffer constant.
/// \param[in] index  The index of the light buffer.
/// \remarks
/// Example: To declare LightBuffer0 and LightBuffer0Sampler call
///   DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
/// Usually you will use
///  DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
///  DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);
#define DECLARE_UNIFORM_LIGHTBUFFER(name, index) \
texture name : LIGHTBUFFER##index; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = NONE; \
}


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Gets the linear depth in the range [0,1] from a G-buffer 0 sample.
/// \param[in] gBuffer0    The G-buffer 0 value.
/// \return The linear depth in the range [0, 1].
float GetGBufferDepth(float4 gBuffer0)
{
  return abs(gBuffer0.x);
}


#if CREATE_GBUFFER
/// Stores the depth in the given G-buffer 0 value.
/// \param[in] depth          The linear depth in the range [0, 1].
/// \param[in] sceneNodeType  The scene node type info (1 = static, 0 = dynamic).
/// \param[in,out] gBuffer0   The G-buffer 0 value.
void SetGBufferDepth(float depth, float sceneNodeType, inout float4 gBuffer0)
{
  if (sceneNodeType)
  {
    // Static objects are encoded as positive values.
    gBuffer0 = depth;
  }
  else
  {
    // Dynamic objects are encoded as negative values.
    gBuffer0 = -depth;
  }
}
#endif


/// Gets the world space normal from a G-buffer 1 sample.
/// \param[in] gBuffer1    The G-buffer 1 value.
/// \return The normal in world space.
float3 GetGBufferNormal(float4 gBuffer1)
{
  return DecodeNormalBestFit((half4)gBuffer1);
  
  //float x = gBuffer1.r * 2 - 1;
  //float y = gBuffer1.g * 2 - 1;
  //return float3(x, y, sqrt(1 - x*x - y*y));
  
  //return normalize(DecodeNormalStereographic(gBuffer1));
  //return normalize(DecodeNormalSphereMap(gBuffer1));
}

#if CREATE_GBUFFER
/// Stores the world space normal in the given G-buffer 1 value.
/// \param[in] normal         The normal in world space.
/// \param[in,out] gBuffer1   The G-buffer 1 value.
void SetGBufferNormal(float3 normal, inout float4 gBuffer1)
{
  gBuffer1.rgb = EncodeNormalBestFit((half3)normal, NormalsFittingSampler);
  
  //gBuffer1.rgb = normal.xyz * 0.5f + 0.5f;
  
  // Note: GBuffer now encodes normal in world space. Does this work for these
  // encodings?
  //gBuffer1.rg = EncodeNormalStereographic(normal);
  //gBuffer1.rg = EncodeNormalSphereMap(normal);
}
#endif


/// Gets the specular power from the given G-buffer samples.
/// \param[in] gBuffer0    The G-buffer 0 value.
/// \param[in] gBuffer1    The G-buffer 1 value.
/// \return The specular power.
float GetGBufferSpecularPower(float4 gBuffer0, float4 gBuffer1)
{
  return DecodeSpecularPower(gBuffer1.a);
}

/// Stores the given specular power in the G-buffer.
/// \param[in] specularPower  The specular power.
/// \param[in,out] gBuffer0   The G-buffer 0 value.
/// \param[in,out] gBuffer1   The G-buffer 1 value.
void SetGBufferSpecularPower(float specularPower, inout float4 gBuffer0, inout float4 gBuffer1)
{
  gBuffer1.a = EncodeSpecularPower(specularPower);
}

/// Gets the diffuse light value from the given light buffer samples.
/// \param[in] lightBuffer0   The light buffer 0 value.
/// \param[in] lightBuffer1   The light buffer 1 value.
/// \return The diffuse light value.
float3 GetLightBufferDiffuse(float4 lightBuffer0, float4 lightBuffer1)
{
  return lightBuffer0.xyz;
}


/// Gets the specular light value from the given light buffer samples.
/// \param[in] lightBuffer0   The light buffer 0 value.
/// \param[in] lightBuffer1   The light buffer 1 value.
/// \return The specular light value.
float3 GetLightBufferSpecular(float4 lightBuffer0, float4 lightBuffer1)
{
  return lightBuffer1.xyz;
}


/// Gets the index of the given texture corner.
/// \param[in] texCoord The texture coordinate of one of the texture corners.
///                     Allowed values are (0, 0), (1, 0), (0, 1), and (1, 1).
/// \return The index of the texture corner.
/// \retval 0   left, top
/// \retval 1   right, top
/// \retval 2   left, bottom
/// \retval 3   right, bottom
float GetCornerIndex(in float2 texCoord)
{
  return texCoord.x + (texCoord.y * 2);
}


struct VSFrustumRayInput
{
  float4 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;    // The texture coordinate of one of the texture corners.
                                  // Allowed values are (0, 0), (1, 0), (0, 1), and (1, 1).
};

struct VSFrustumRayOutput
{
  float2 TexCoord : TEXCOORD0;    // The texture coordinates of the vertex.
  float3 FrustumRay : TEXCOORD1;
  float4 Position : SV_Position;
};

/// A vertex shader that also converts the position from screen space for clip space and computes
/// the frustum ray for this vertex.
/// \param[in] input            The vertex data (see VSFrustumRayInput).
/// \param[in] viewportSize     The viewport size in pixels.
/// \param[in] frustumCorners   See constant FrustumCorners above.
VSFrustumRayOutput VSFrustumRay(VSFrustumRayInput input,
                                uniform const float2 viewportSize,
                                uniform const float3 frustumCorners[4])
{
  float4 position = input.Position;
  float2 texCoord = input.TexCoord;
  
  position.xy /= viewportSize;
  
  texCoord.xy = position.xy;
  
  // Instead of subtracting the 0.5 pixel offset from the position, we add
  // it to the texture coordinates - because frustumRay is associated with
  // the position output.
#if !SM4
  texCoord.xy += 0.5f / viewportSize;
#endif
  
  position.xy = position.xy * float2(2, -2) - float2(1, -1);
  
  VSFrustumRayOutput output = (VSFrustumRayOutput)0;
  output.Position = position;
  output.TexCoord = texCoord;
  output.FrustumRay = frustumCorners[GetCornerIndex(input.TexCoord)];
  
  return output;
}


/// Reconstructs the position in view space.
/// \param[in]  texCoord    The texture coordinates of the current pixel.
/// \param[in]  depth       The depth [0, Far] of the current pixel in view space.
/// \param[in]  frustumInfo The frustum info. See DECLARE_UNIFORM_FRUSTUMINFO().
/// \return The position in view space.
float3 GetPositionView(float2 texCoord, float depth, float4 frustumInfo)
{
  float3 frustumRay = float3(frustumInfo.x + texCoord.x * frustumInfo.z,
                             frustumInfo.y + texCoord.y * frustumInfo.w,
                             -1);
  return depth * frustumRay;
}


/// Reconstructs the normal from the given position.
/// \param[in]  positionView  The position in.
/// \return The normal in view space.
/// \remarks
/// The function returns the normal of the same space as the given position
/// (e.g. world space or view space).
float3 DeriveNormal(float3 position)
{
  return normalize(cross(ddy(position), ddx(position)));
}
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

float HalfEdgeWidth = 1.0;
float DepthThreshold = 0.05;
float DepthSensitivity = 10000;
float NormalThreshold = 0.99;
float NormalSensitivity = 1;
float3 CameraBackward;       // The camera backward vector in world space.

// A silhouette edge is a depth discontinuity.
float4 SilhouetteColor = float4(0, 0, 0, 1);

// A crease edge is a normal discontinuity.
float4 CreaseColor = float4(1, 1, 1, 1);


// The input texture.
Texture2D SourceTexture;
sampler2D SourceSampler : register(s0) = sampler_state
{
  Texture = <SourceTexture>;
};

// The depth buffer.
Texture2D GBuffer0;
sampler2D GBuffer0Sampler = sampler_state
{
  Texture = <GBuffer0>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// The normal buffer.
Texture2D GBuffer1;
sampler2D GBuffer1Sampler = sampler_state
{
  Texture = <GBuffer1>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
};

struct VSOutput
{
  float2 TexCoord : TEXCOORD;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


/// Detects 1-pixel edges using depth and normal information in G-buffer.
/// Silhouette edges are depth discontinuities. Crease edges are normal discontinuities.
/// The 1-pixel edge is drawn on the pixel a that is in front.
/// \param[in]  texCoord    The texture coordinate.
/// \param[out] silhouette  The intensity of the silhouette edge at the current pixel.
/// \param[out] crease      The intensity of the crease edge at the current pixel.
void DetectOnePixelEdge(float2 texCoord, out float silhouette, out float crease)
{
  // ----- Edge detection using horizontal and vertical samples
  //
  //   -|----|----|----|-
  //    |    | s0 |    |
  //   -|----|----|----|-
  //    | s1 | s  | s2 |
  //   -|----|----|----|-
  //    |    | s3 |    |
  //   -|----|----|----|-
  //
  float2 offset = 1 / ViewportSize;
  
  float z = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0)));
  float4 zs;
  zs.x = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(0, -1) * offset, 0, 0)));
  zs.y = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(-1, 0) * offset, 0, 0)));
  zs.z = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(1, 0) * offset, 0, 0)));
  zs.w = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(0, 1) * offset, 0, 0)));
  
  float3 n = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord, 0, 0)));
  float3 n0 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(0, -1) * offset, 0, 0)));
  float3 n1 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(-1, 0) * offset, 0, 0)));
  float3 n2 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(1, 0) * offset, 0, 0)));
  float3 n3 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(0, 1) * offset, 0, 0)));
  
  // ----- Depth Threshold
  // Artifacts appear when
  //
  //  n --> 90�  (The normal goes to 90� in view space.)
  //    AND
  //  z --> 1    (The sample depth goes to 1.)
  //
  // The first condition can be modeled as: (1 - n.z)
  // The second condition can be modeled as: depth^i (with i = 2 giving optimal results)
  // AND is modeled as multiplication.
  //
  // Increase the depth threshold to prevent artifacts:
  // Normal is in world space. To get n.z in view space:
  float nz = dot(CameraBackward, n);
  float depthThreshold = lerp(DepthThreshold, 1, (1 - nz) * z * z);
  
  // Optional: Increase normal threshold in the distance.
  float normalThreshold = NormalThreshold;
  //normalThreshold = lerp(NormalThreshold, 2, z * z);
  
  // ----- Silhouette (depth discontinuity)
  float4 dz = saturate(zs - z);
  dz = saturate((dz - depthThreshold) * DepthSensitivity);
  
  // Optional: Use d� instead of d to create a sharper transition.
  //dz = dz * dz;
  
  // Take dz.x OR dz.y OR dz.z OR dz.w.
  dz = 1 - dz;
  silhouette = 1 - dz.x * dz.y * dz.z * dz.w;
  
  // ----- Crease (normal discontinuity)
  float4 dn = float4(dot(n, n0), dot(n, n1), dot(n, n2), dot(n, n3));
  dn = 1 - dn;
  
  // Edge should only be drawn on pixel that is in front.
  float4 isInFront = (z < zs);
  dn *= isInFront;
  
  // Skybox pixels do not have valid normals. Ignore them.
  const float skyBoxLimit = 0.99999;
  float4 isNotSkyBox = (zs <= skyBoxLimit);
  dn *= isNotSkyBox;
  
  dn = saturate((dn - normalThreshold) * NormalSensitivity);
  
  // Take average of normal deltas.
  // crease = (dn.x + dn.y + dn.z + dn.w) / 4;
  crease = dot(dn, 0.25);
}


/// Detects edges using depth and normal information in G-buffer.
/// Silhouette edges are depth discontinuities. Crease edges are normal discontinuities.
/// \param[in]  texCoord    The texture coordinate.
/// \param[out] silhouette  The intensity of the silhouette edge at the current pixel.
/// \param[out] crease      The intensity of the crease edge at the current pixel.
void DetectEdge(float2 texCoord, out float silhouette, out float crease)
{
  // ----- Edge detection using diagonal samples
  //
  //   -|----|----|----|-
  //    | s0 |    | s1 |
  //   -|----|----|----|-
  //    |    | s  |    |
  //   -|----|----|----|-
  //    | s2 |    | s3 |
  //   -|----|----|----|-
  //
  // According to Nienhaus and D�llner: "Edge-Enhancement - An Algorithm for Real-Time Non-Photorealistic Rendering"
  // diagonal samples yield better results than sampling all 8 directions. This coincides with our own findings.
  
  // The sample offset is scaled by edge width.
  float2 offset = HalfEdgeWidth / ViewportSize;
  
  float z = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0)));
  float z0 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(-1, -1) * offset, 0, 0)));
  float z1 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(1, -1) * offset, 0, 0)));
  float z2 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(-1, 1) * offset, 0, 0)));
  float z3 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(1, 1) * offset, 0, 0)));
  float3 n = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord, 0, 0)));
  float3 n0 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(-1, -1) * offset, 0, 0)));
  float3 n1 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(1, -1) * offset, 0, 0)));
  float3 n2 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(-1, 1) * offset, 0, 0)));
  float3 n3 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(1, 1) * offset, 0, 0)));
  
  // ----- Depth Threshold
  // Artifacts appear when
  //
  //  n --> 90�  (The normal goes to 90� in view space.)
  //    AND
  //  z --> 1    (The sample depth goes to 1.)
  //
  // The first condition can be modeled as: (1 - n.z)
  // The second condition can be modeled as: depth^i (with i = 2 giving optimal results)
  // AND is modeled as multiplication.
  //
  // Increase the depth threshold to prevent artifacts:
  // Normal is in world space. To get n.z in view space:
  float nz = dot(CameraBackward, n);
  float depthThreshold = lerp(DepthThreshold, 1, (1 - nz) * z * z);
  
  // Optional: Increase normal threshold in the distance.
  float normalThreshold = NormalThreshold;
  //normalThreshold = lerp(NormalThreshold, 2, z * z);
  
  // ----- Silhouette (depth discontinuity)
  float dz0 = abs(z3 - z0); // Delta along -45� diagonal.
  float dz1 = abs(z1 - z2); // Delta along +45� diagonal.
  dz0 = saturate((dz0 - depthThreshold) * DepthSensitivity);
  dz1 = saturate((dz1 - depthThreshold) * DepthSensitivity);
  
  // Optional: Use d� instead of d to create a sharper transition.
  //dz0 = dz0 * dz0;
  //dz1 = dz1 * dz1;
  
  // Take dz0 OR dz1.
  silhouette = 1 - (1 - dz0) * (1 - dz1);
  
  // ----- Crease (normal discontinuity)
  float dn0 = 1 - dot(n0, n3); // Delta along -45� diagonal.
  float dn1 = 1 - dot(n1, n2); // Delta along +45� diagonal.
  
  // Skybox pixels do not have valid normals. Ignore them.
  const float skyBoxLimit = 0.99999;
  //if (z0 > skyBoxLimit || z3 > skyBoxLimit)
  //  dn0 = 0;
  //if (z1 > skyBoxLimit || z2 > skyBoxLimit)
  //  dn1 = 0;
  // Optimized:
  float4 isNotSkyBox =  (float4(z0, z1, z2, z3) <= skyBoxLimit);
  dn0 *= isNotSkyBox.x * isNotSkyBox.w;
  dn1 *= isNotSkyBox.y * isNotSkyBox.z;
  
  dn0 = saturate((dn0 - normalThreshold) * NormalSensitivity);
  dn1 = saturate((dn1 - normalThreshold) * NormalSensitivity);
  
  // Take average of normal deltas.
  crease = 0.5 * (dn0 + dn1);
}


float4 PS(in float2 texCoord : TEXCOORD0, bool onePixelEdge) : COLOR0
{
  float silhouette, crease;
  if (onePixelEdge)
    DetectOnePixelEdge(texCoord, silhouette, crease);
  else
    DetectEdge(texCoord, silhouette, crease);
  
  // Debugging: Output silhouette image.
  //return float4(silhouette, silhouette, silhouette, 1);
  
  // Debugging: Output crease image.
  //return float4(crease, crease, crease, 1);
  
  // Debugging: Output combined edge image.
  //float edge = 1 - (1 - silhouette) * (1 - crease);
  //return float4(edge, edge, edge, 1);
  
  float4 color = tex2D(SourceSampler, texCoord);
  float3 edgeColor;
  float edgeFactor;
  if (silhouette)
  {
    edgeColor = SilhouetteColor.rgb;
    edgeFactor = silhouette * SilhouetteColor.a;
  }
  else
  {
    edgeColor = CreaseColor.rgb;
    edgeFactor = crease * CreaseColor.a;
  }
  
  // Option A: Draw silhouette and crease edges.
  color.rgb = lerp(color.rgb, edgeColor, edgeFactor);
  
  // Option B: Modulate source image.
  // Lerp between source color and 2X multiplicative blending.
  //color.rgb = lerp(color.rgb, 2 * color.rgb * edgeColor, edgeFactor);
  
  return color;
}

float4 PSEdge(in float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord, false); }
float4 PSOnePixelEdge(in float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord, true); }


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_4_0
#define PSTARGET ps_4_0
#endif

technique
{
  pass Edge
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSEdge();
  }
  
  pass OnePixelEdge
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSOnePixelEdge();
  }
}
