using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADOFAI;
using UnityEngine;

namespace NeoEditor
{
    public static class NeoConstants
    {
        public static readonly LevelEventType[] TimelineIgnoreEvents = new LevelEventType[]
        {
            LevelEventType.None,
            LevelEventType.SetSpeed,
            LevelEventType.Twirl,
            LevelEventType.Checkpoint,
            LevelEventType.LevelSettings,
            LevelEventType.SongSettings,
            LevelEventType.TrackSettings,
            LevelEventType.BackgroundSettings,
            LevelEventType.CameraSettings,
            LevelEventType.MiscSettings,
            LevelEventType.EventSettings,
            LevelEventType.DecorationSettings,
            LevelEventType.AddDecoration,
            LevelEventType.AddText,
            LevelEventType.Hold,
            LevelEventType.CallMethod,
            LevelEventType.AddComponent,
            LevelEventType.MultiPlanet,
            LevelEventType.FreeRoam,
            LevelEventType.FreeRoamTwirl,
            LevelEventType.FreeRoamRemove,
            LevelEventType.FreeRoamWarning,
            LevelEventType.Pause,
            LevelEventType.AutoPlayTiles,
            LevelEventType.ScaleMargin,
            LevelEventType.ScaleRadius,
            LevelEventType.Multitap,
            LevelEventType.TileDimensions,
            LevelEventType.KillPlayer,
            LevelEventType.SetFloorIcon,
            LevelEventType.AddObject,
            LevelEventType.AddParticle
        };

        public static readonly LevelEventType[] SelectorIgnoreEvents = new LevelEventType[]
        {
            LevelEventType.None,
            LevelEventType.LevelSettings,
            LevelEventType.SongSettings,
            LevelEventType.TrackSettings,
            LevelEventType.BackgroundSettings,
            LevelEventType.CameraSettings,
            LevelEventType.MiscSettings,
            LevelEventType.EventSettings,
            LevelEventType.DecorationSettings,
            LevelEventType.Multitap,
            LevelEventType.SetSpeed,
            LevelEventType.Twirl,
            LevelEventType.Pause,
            LevelEventType.Hold,
            LevelEventType.MultiPlanet,
            LevelEventType.FreeRoam,
            LevelEventType.AddDecoration,
            LevelEventType.AddText,
            LevelEventType.AddObject,
            LevelEventType.AddParticle
        };

        public static readonly LevelEventType[] SoundEvents = new LevelEventType[]
        {
            LevelEventType.SetHitsound,
            LevelEventType.PlaySound,

            LevelEventType.SetHoldSound
        };

        public static readonly LevelEventType[] PlanetEvents = new LevelEventType[]
        {
            LevelEventType.SetPlanetRotation,
            LevelEventType.ScalePlanets,

            LevelEventType.ScaleRadius,
            LevelEventType.ScaleMargin,
        };

        public static readonly LevelEventType[] TrackEvents = new LevelEventType[]
        {
            LevelEventType.ChangeTrack,
            LevelEventType.ColorTrack,
            LevelEventType.AnimateTrack,
            LevelEventType.RecolorTrack,
            LevelEventType.MoveTrack,
            LevelEventType.PositionTrack,

            LevelEventType.Hide
        };

        public static readonly LevelEventType[] DecorationEvents = new LevelEventType[]
        {
            LevelEventType.AddDecoration,
            LevelEventType.AddText,
            LevelEventType.AddObject,
            LevelEventType.AddParticle,
            LevelEventType.MoveDecorations,
            LevelEventType.SetText,
            LevelEventType.SetObject,
            LevelEventType.SetParticle,
            LevelEventType.EmitParticle,
            LevelEventType.SetDefaultText,
        };

        public static readonly LevelEventType[] CameraEvents = new LevelEventType[]
        {
            LevelEventType.MoveCamera,
            LevelEventType.ShakeScreen,
            LevelEventType.ScreenTile,
            LevelEventType.ScreenScroll
        };

        public static readonly LevelEventType[] FilterEvents = new LevelEventType[]
        {
            LevelEventType.CustomBackground,
            LevelEventType.Flash,
            LevelEventType.SetFilter,
            LevelEventType.HallOfMirrors,
            LevelEventType.Bloom,
            (LevelEventType)61 // Set Frame Rate
        };

        public static readonly LevelEventType[] ModifierEvents = new LevelEventType[]
        {
            LevelEventType.RepeatEvents,
            LevelEventType.SetConditionalEvents,
			LevelEventType.EditorComment,
			LevelEventType.Bookmark
		};

		public static readonly Dictionary<LevelEventCategory, LevelEventType[]> Events = new()
		{
			{ LevelEventCategory.Sound, SoundEvents },
			{ LevelEventCategory.Planet, PlanetEvents },
			{ LevelEventCategory.Track, TrackEvents },
			{ LevelEventCategory.Decoration, DecorationEvents },
			{ LevelEventCategory.Camera, CameraEvents },
			{ LevelEventCategory.Filter, FilterEvents },
			{ LevelEventCategory.Modifier, ModifierEvents }
		};

        public static readonly Dictionary<LevelEventCategory, Sprite> CategoryIcons = new()
        {
			{ LevelEventCategory.Sound, GCS.levelEventIcons[LevelEventType.PlaySound] },
			{ LevelEventCategory.Planet, GCS.levelEventIcons[LevelEventType.SetPlanetRotation] },
			{ LevelEventCategory.Track, GCS.levelEventIcons[LevelEventType.ChangeTrack] },
			{ LevelEventCategory.Decoration, GCS.levelEventIcons[LevelEventType.AddDecoration] },
			{ LevelEventCategory.Camera, GCS.levelEventIcons[LevelEventType.MoveCamera] },
			{ LevelEventCategory.Filter, GCS.levelEventIcons[LevelEventType.SetFilter] },
			{ LevelEventCategory.Modifier, GCS.levelEventIcons[LevelEventType.SetConditionalEvents] },
		};

		public static readonly Color DefaultEventColor = new Color(0.690196f, 0.690196f, 0.690196f);
		public static readonly Color SoundEventColor = new Color(1f, 0.196078f, 0.196078f);
		public static readonly Color PlanetEventColor = new Color(0.196078f, 0.470588f, 1f);
		public static readonly Color TrackEventColor = new Color(1f, 0.870588f, 0.415686f);
		public static readonly Color DecorationEventColor = new Color(0.435294f, 0.933333f, 0.423529f);
		public static readonly Color CameraEventColor = new Color(1f, 0.415686f, 0.941176f);
		public static readonly Color FilterEventColor = new Color(0.368627f, 0.886274f, 1f);
		public static readonly Color ModifierEventColor = new Color(1f, 0.709803f, 0.368627f);
		public static readonly Color CommentEventColor = new Color(0.760784f, 0.368627f, 1f);

		public static readonly Dictionary<LevelEventType, Color> EventColors = new Dictionary<LevelEventType, Color>()
		{
			{ LevelEventType.None, DefaultEventColor },
			{ LevelEventType.SetHitsound, SoundEventColor },
			{ LevelEventType.PlaySound, SoundEventColor },
			{ LevelEventType.SetHoldSound, SoundEventColor },
			{ LevelEventType.SetPlanetRotation, PlanetEventColor },
			{ LevelEventType.ScalePlanets, PlanetEventColor },
			{ LevelEventType.ScaleRadius, PlanetEventColor },
			{ LevelEventType.ScaleMargin, PlanetEventColor },
			{ LevelEventType.ChangeTrack, TrackEventColor },
			{ LevelEventType.ColorTrack, TrackEventColor },
			{ LevelEventType.AnimateTrack, TrackEventColor },
			{ LevelEventType.RecolorTrack, TrackEventColor },
			{ LevelEventType.MoveTrack, TrackEventColor },
			{ LevelEventType.PositionTrack, TrackEventColor },
			{ LevelEventType.Hide, TrackEventColor },
			{ LevelEventType.AddDecoration, DecorationEventColor },
			{ LevelEventType.AddText, DecorationEventColor },
			{ LevelEventType.AddObject, DecorationEventColor },
            { LevelEventType.AddParticle, DecorationEventColor },
			{ LevelEventType.MoveDecorations, DecorationEventColor },
			{ LevelEventType.SetText, DecorationEventColor },
			{ LevelEventType.SetObject, DecorationEventColor },
            { LevelEventType.SetParticle, DecorationEventColor },
            { LevelEventType.EmitParticle, DecorationEventColor },
			{ LevelEventType.SetDefaultText, DecorationEventColor },
			{ LevelEventType.MoveCamera, CameraEventColor },
			{ LevelEventType.ShakeScreen, CameraEventColor },
			{ LevelEventType.ScreenTile, CameraEventColor },
			{ LevelEventType.ScreenScroll, CameraEventColor },
			{ LevelEventType.CustomBackground, FilterEventColor },
			{ LevelEventType.Flash, FilterEventColor },
			{ LevelEventType.SetFilter, FilterEventColor },
			{ LevelEventType.SetFilterAdvanced, FilterEventColor },
			{ LevelEventType.HallOfMirrors, FilterEventColor },
			{ LevelEventType.Bloom, FilterEventColor },
			{ LevelEventType.SetFrameRate, FilterEventColor }, // Set Frame Rate
            { LevelEventType.RepeatEvents, ModifierEventColor },
			{ LevelEventType.SetConditionalEvents, ModifierEventColor },
			{ LevelEventType.EditorComment, CommentEventColor },
			{ LevelEventType.Bookmark, CommentEventColor }
		};

        public static readonly string DefaultLayout = "AAEAAAD/////AQAAAAAAAAAMAgAAAExEeW5hbWljUGFuZWxzLlJ1bnRpbWUsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxsBQEAAAAxRHluYW1pY1BhbmVscy5QYW5lbFNlcmlhbGl6YXRpb24rU2VyaWFsaXplZENhbnZhcwQAAAAGYWN0aXZlDHVzZUZyZWVTcGFjZQ5yb290UGFuZWxHcm91cBR1bmFuY2hvcmVkUGFuZWxHcm91cAAABAQBATVEeW5hbWljUGFuZWxzLlBhbmVsU2VyaWFsaXphdGlvbitTZXJpYWxpemVkUGFuZWxHcm91cAIAAAA1RHluYW1pY1BhbmVscy5QYW5lbFNlcmlhbGl6YXRpb24rU2VyaWFsaXplZFBhbmVsR3JvdXACAAAAAgAAAAEACQMAAAAJBAAAAAUDAAAANUR5bmFtaWNQYW5lbHMuUGFuZWxTZXJpYWxpemF0aW9uK1NlcmlhbGl6ZWRQYW5lbEdyb3VwAwAAAApob3Jpem9udGFsCGNoaWxkcmVuBHNpemUABAQBNUR5bmFtaWNQYW5lbHMuUGFuZWxTZXJpYWxpemF0aW9uK0lTZXJpYWxpemVkRWxlbWVudFtdAgAAADJEeW5hbWljUGFuZWxzLlBhbmVsU2VyaWFsaXphdGlvbitTZXJpYWxpemVkVmVjdG9yMgIAAAACAAAAAQkFAAAABfr///8yRHluYW1pY1BhbmVscy5QYW5lbFNlcmlhbGl6YXRpb24rU2VyaWFsaXplZFZlY3RvcjICAAAAAXgBeQAACwsCAAAAAADwRADgg0QBBAAAAAMAAAAACQcAAAAB+P////r///8AAEhCAABIQgcFAAAAAAEAAAACAAAABDNEeW5hbWljUGFuZWxzLlBhbmVsU2VyaWFsaXphdGlvbitJU2VyaWFsaXplZEVsZW1lbnQCAAAACQkAAAAJCgAAAAcHAAAAAAEAAAABAAAABDNEeW5hbWljUGFuZWxzLlBhbmVsU2VyaWFsaXphdGlvbitJU2VyaWFsaXplZEVsZW1lbnQCAAAACQsAAAABCQAAAAMAAAAACQwAAAAB8/////r///8AAMhEAOCDRAEKAAAAAwAAAAAJDgAAAAHx////+v///wAAoEMA4INEBQsAAAA1RHluYW1pY1BhbmVscy5QYW5lbFNlcmlhbGl6YXRpb24rU2VyaWFsaXplZER1bW15UGFuZWwBAAAABHNpemUEMkR5bmFtaWNQYW5lbHMuUGFuZWxTZXJpYWxpemF0aW9uK1NlcmlhbGl6ZWRWZWN0b3IyAgAAAAIAAAAB8P////r///8AAEhCAABIQgcMAAAAAAEAAAACAAAABDNEeW5hbWljUGFuZWxzLlBhbmVsU2VyaWFsaXphdGlvbitJU2VyaWFsaXplZEVsZW1lbnQCAAAACREAAAAJEgAAAAcOAAAAAAEAAAACAAAABDNEeW5hbWljUGFuZWxzLlBhbmVsU2VyaWFsaXphdGlvbitJU2VyaWFsaXplZEVsZW1lbnQCAAAACRMAAAAJFAAAAAURAAAAMER5bmFtaWNQYW5lbHMuUGFuZWxTZXJpYWxpemF0aW9uK1NlcmlhbGl6ZWRQYW5lbAQAAAAJYWN0aXZlVGFiBHRhYnMMZmxvYXRpbmdTaXplBHNpemUABAQECDVEeW5hbWljUGFuZWxzLlBhbmVsU2VyaWFsaXphdGlvbitTZXJpYWxpemVkUGFuZWxUYWJbXQIAAAAyRHluYW1pY1BhbmVscy5QYW5lbFNlcmlhbGl6YXRpb24rU2VyaWFsaXplZFZlY3RvcjICAAAAMkR5bmFtaWNQYW5lbHMuUGFuZWxTZXJpYWxpemF0aW9uK1NlcmlhbGl6ZWRWZWN0b3IyAgAAAAIAAAAAAAAACRUAAAAB6v////r///8AAMhEAIDzQwHp////+v///wAAyESFI/pDARIAAAADAAAAAQkYAAAAAef////6////AADIRD2uCkQBEwAAABEAAAAAAAAACRoAAAAB5f////r///8AAKBDAEBiRAHk////+v///wAAoEMAQFtEARQAAAARAAAAAAAAAAkdAAAAAeL////6////AACgQwAAMkMB4f////r///8AAKBDAAAyQwcVAAAAAAEAAAABAAAABDNEeW5hbWljUGFuZWxzLlBhbmVsU2VyaWFsaXphdGlvbitTZXJpYWxpemVkUGFuZWxUYWICAAAACSAAAAAHGAAAAAABAAAAAwAAAAQzRHluYW1pY1BhbmVscy5QYW5lbFNlcmlhbGl6YXRpb24rSVNlcmlhbGl6ZWRFbGVtZW50AgAAAAkhAAAACSIAAAAJIwAAAAcaAAAAAAEAAAABAAAABDNEeW5hbWljUGFuZWxzLlBhbmVsU2VyaWFsaXphdGlvbitTZXJpYWxpemVkUGFuZWxUYWICAAAACSQAAAAHHQAAAAABAAAAAQAAAAQzRHluYW1pY1BhbmVscy5QYW5lbFNlcmlhbGl6YXRpb24rU2VyaWFsaXplZFBhbmVsVGFiAgAAAAklAAAABSAAAAAzRHluYW1pY1BhbmVscy5QYW5lbFNlcmlhbGl6YXRpb24rU2VyaWFsaXplZFBhbmVsVGFiAwAAAAJpZAdtaW5TaXplBWxhYmVsAQQBMkR5bmFtaWNQYW5lbHMuUGFuZWxTZXJpYWxpemF0aW9uK1NlcmlhbGl6ZWRWZWN0b3IyAgAAAAIAAAAGJgAAAAhUaW1lbGluZQHZ////+v///wAAyEMAAJZDCSYAAAABIQAAABEAAAAAAAAACSgAAAAB1/////r///8AAKBDAAAORAHW////+v///wAAoEM9rgpEASIAAAARAAAAAAAAAAkrAAAAAdT////6////AACgQwAADkQB0/////r///8AAKBDPa4KRAEjAAAAEQAAAAAAAAAJLgAAAAHR////+v///wAAcEQAAAdEAdD////6////AABwRD2uCkQBJAAAACAAAAAGMQAAAAlJbnNwZWN0b3IBzv////r///8AAHpDAABIQwkxAAAAASUAAAAgAAAABjMAAAAEUGxheQHM////+v///wAAekMAABZDCTMAAAAHKAAAAAABAAAAAQAAAAQzRHluYW1pY1BhbmVscy5QYW5lbFNlcmlhbGl6YXRpb24rU2VyaWFsaXplZFBhbmVsVGFiAgAAAAk1AAAABysAAAAAAQAAAAIAAAAEM0R5bmFtaWNQYW5lbHMuUGFuZWxTZXJpYWxpemF0aW9uK1NlcmlhbGl6ZWRQYW5lbFRhYgIAAAAJNgAAAAk3AAAABy4AAAAAAQAAAAIAAAAEM0R5bmFtaWNQYW5lbHMuUGFuZWxTZXJpYWxpemF0aW9uK1NlcmlhbGl6ZWRQYW5lbFRhYgIAAAAJOAAAAAk5AAAAATUAAAAgAAAABjoAAAALRGVjb3JhdGlvbnMBxf////r///8AAHpDAABIQwk6AAAAATYAAAAgAAAABjwAAAAHUHJvamVjdAHD////+v///wAAekMAAEhDCTwAAAABNwAAACAAAAAGPgAAAAVNZWRpYQHB////+v///wAAekMAAEhDCT4AAAABOAAAACAAAAAGQAAAAAdQcmV2aWV3Ab/////6////AACgQwAANEMJQAAAAAE5AAAAIAAAAAZCAAAABVNjZW5lAb3////6////AACgQwAANEMJQgAAAAs=";
	}
}
