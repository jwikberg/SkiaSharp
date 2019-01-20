﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HarfBuzzSharp
{
	public class Font : NativeObject
	{
		internal Font (IntPtr handle)
			: base (handle)
		{
		}

		public Font (Face face)
			: this (IntPtr.Zero)
		{
			if (face == null) {
				throw new ArgumentNullException (nameof (face));
			}

			Handle = HarfBuzzApi.hb_font_create (face.Handle);
		}

		public FontExtents HorizontalFontExtents {
			get {
				if (HarfBuzzApi.hb_font_get_h_extents (Handle, out var fontExtents)) {
					return fontExtents;
				}
				return new FontExtents ();
			}
		}

		public FontExtents VerticalFontExtents {
			get {
				if (HarfBuzzApi.hb_font_get_v_extents (Handle, out var fontExtents)) {
					return fontExtents;
				}
				return new FontExtents ();
			}
		}

		public uint GetHorizontalGlyphAdvance (uint glyph)
		{
			return HarfBuzzApi.hb_font_get_glyph_h_advance (Handle, glyph);
		}

		public uint GetVerticalGlyphAdvance (uint glyph)
		{
			return HarfBuzzApi.hb_font_get_glyph_v_advance (Handle, glyph);
		}

		public uint GetGlyph (uint unicode, uint variationSelector = 0)
		{
			if (HarfBuzzApi.hb_font_get_glyph (Handle, unicode, variationSelector, out var glyph)) {
				return glyph;
			}

			return 0;
		}

		public string GetGlyphName (uint glyph)
		{
			var builder = new StringBuilder ();

			if (HarfBuzzApi.hb_font_get_glyph_name (Handle, glyph, builder, (uint)builder.Length)) {
				return builder.ToString ();
			}

			return string.Empty;
		}

		public GlyphExtents GetGlyphExtents (uint glyph)
		{
			if (HarfBuzzApi.hb_font_get_glyph_extents (Handle, glyph, out var extents)) {
				return extents;
			}
			return new GlyphExtents ();
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_font_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		public void SetScale (int xScale, int yScale) => HarfBuzzApi.hb_font_set_scale (Handle, xScale, yScale);

		public void GetScale (out int xScale, out int yScale) => HarfBuzzApi.hb_font_get_scale (Handle, out xScale, out yScale);

		public void SetFunctionsOpenType () => HarfBuzzApi.hb_ot_font_set_funcs (Handle);

		public void Shape (Buffer buffer, params Feature[] features)
		{
			if (buffer == null) {
				throw new ArgumentNullException (nameof (buffer));
			}

			if (features == null || features.Length == 0) {
				HarfBuzzApi.hb_shape (Handle, buffer.Handle, IntPtr.Zero, 0);
			} else {
				var ptr = StructureArrayToPtr (features);
				HarfBuzzApi.hb_shape (Handle, buffer.Handle, ptr, (uint)features.Length);
				Marshal.FreeCoTaskMem (ptr);
			}
		}

		public void ShapeFull (Buffer buffer, IReadOnlyList<Feature> features = null, IReadOnlyList<string> shapers = null)
		{
			if (buffer == null) {
				throw new ArgumentNullException (nameof (buffer));
			}

			var featuresPtr = features == null || features.Count == 0 ? IntPtr.Zero : StructureArrayToPtr (features);
			var shapersPtr = shapers == null || shapers.Count == 0 ? IntPtr.Zero : StructureArrayToPtr (shapers);

			HarfBuzzApi.hb_shape_full (
				Handle,
				buffer.Handle,
				IntPtr.Zero,
				featuresPtr != IntPtr.Zero ? (uint)features.Count : 0,
				shapersPtr);

			if (featuresPtr != IntPtr.Zero) {
				Marshal.FreeCoTaskMem (featuresPtr);
			}

			if (shapersPtr != IntPtr.Zero) {
				Marshal.FreeCoTaskMem (shapersPtr);
			}
		}

		public IReadOnlyList<string> SupportedShapers {
			get {
				return PtrToStringArray (HarfBuzzApi.hb_shape_list_shapers ()).ToArray ();
			}
		}
	}
}
