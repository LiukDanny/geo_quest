#ifndef COUNTRY_OUTLINE_INCLUDED
#define COUNTRY_OUTLINE_INCLUDED

void CountryOutline_float(
    Texture2D CountryTex,
    SamplerState CountrySampler,
    float2 UV,
    float4 SelectedColor,
    float OutlineWidth,
    out float Outline)
{
    float2 texelSize = 1.0 / CountryTex.texelSize.zw;

    float2 offsetX = float2(texelSize.x * OutlineWidth, 0);
    float2 offsetY = float2(0, texelSize.y * OutlineWidth);

    float4 c = SAMPLE_TEXTURE2D(CountryTex, CountrySampler, UV);
    float4 r = SAMPLE_TEXTURE2D(CountryTex, CountrySampler, UV + offsetX);
    float4 l = SAMPLE_TEXTURE2D(CountryTex, CountrySampler, UV - offsetX);
    float4 u = SAMPLE_TEXTURE2D(CountryTex, CountrySampler, UV + offsetY);
    float4 d = SAMPLE_TEXTURE2D(CountryTex, CountrySampler, UV - offsetY);

    float center = distance(c.rgb, SelectedColor.rgb) < 0.001;
    float right = distance(r.rgb, SelectedColor.rgb) < 0.001;
    float left = distance(l.rgb, SelectedColor.rgb) < 0.001;
    float up = distance(u.rgb, SelectedColor.rgb) < 0.001;
    float down = distance(d.rgb, SelectedColor.rgb) < 0.001;

    float expanded = max(center, max(right, max(left, max(up, down))));

    Outline = saturate(expanded - center);
}

#endif