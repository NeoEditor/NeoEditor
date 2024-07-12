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
            LevelEventType.AddObject
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
            LevelEventType.CallMethod,
            LevelEventType.AddComponent,
            LevelEventType.FreeRoamWarning,
            LevelEventType.Multitap,
            LevelEventType.KillPlayer,
            LevelEventType.SetFloorIcon
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
            LevelEventType.MoveDecorations,
            LevelEventType.SetText,
            LevelEventType.SetObject,
            LevelEventType.SetDefaultText
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
	}
}
