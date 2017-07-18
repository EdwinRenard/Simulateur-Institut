#pragma once
#ifndef _DBOX_EEVENTMEANING_H_INCLUDED_
#define _DBOX_EEVENTMEANING_H_INCLUDED_
//------------------------------------------------------------------------
// D-BOX LIVE MOTION API
// Copyright (C) 2008, D-BOX Technologies Inc.
// All rights reserved.
//------------------------------------------------------------------------

namespace dbox {
	//------------------------------------------------------------------------
	/// This enum is the list of known meanings that can be associated to an event
	/// during registration.
	/// D-BOX may add meanings to this list over time but should not remove any.
	/// A same event meaning can be associated to multiple event keys.
	//------------------------------------------------------------------------
	enum EEventMeaning {
		EM_UNKNOWN = 0,

		/// Configuration update event (changing vehicle or context).
		EM_CONFIG_UPDATE = 1,
		/// Frame update event (generally at each simulation frame).
		EM_FRAME_UPDATE = 2,

		/// Vehicle engine started.
		EM_ENGINE_START = 3,
		/// Vehicle engine stopped.
		EM_ENGINE_STOP = 4,
		/// Vehicle engine boost (or nitro) action started.
		EM_ENGINE_BOOST_START = 5,
		/// Vehicle engine boost (or nitro) action ended.
		EM_ENGINE_BOOST_STOP = 6,

		/// General impact
		EM_IMPACT = 7,

		/// Register an action to global list using ACTION field meanings.
		/// FM_ACTION (unique identifier) is mandatory.
		/// If unique identifier is already registered, it will be overwritten with new action.
		EM_ACTION_REGISTER = 8,
		/// Unregister an action from global list.
		/// Only FM_ACTION (unique identifier) is mandatory.
		EM_ACTION_UNREGISTER = 9,
		/// Unregister all actions from global list.
		EM_ACTION_UNREGISTER_ALL = 10,
		/// Assign an action to one of the 8 available slots, ready for use.
		/// FM_ACTION (unique identifier) must be specified along with
		/// FM_ACTION_SLOT to which action is to be assigned.
		/// If an action is already assigned to specified slot, assignment will be overwritten.
		EM_ACTION_ASSIGN = 11,
		/// Unassign an action from one of the 8 available slots.
		/// Only FM_ACTION_SLOT is mandatory.
		EM_ACTION_UNASSIGN = 12,
		/// Unassign actions from all slots.
		EM_ACTION_UNASSIGN_ALL = 13,
		/// Send an action trigger pulse (i.e. one-shot weapon fire)
		EM_ACTION_TRIGGER_PULSE = 14,
		/// Send an action trigger start (i.e. start of continuous weapon firing)
		EM_ACTION_TRIGGER_START = 15,
		/// Send an action trigger stop (i.e. end of continuous weapon firing)
		EM_ACTION_TRIGGER_STOP = 16,


		//------------------------------------------------------------------------
		// Cine Motion Data
		//------------------------------------------------------------------------
		/// Cinemotion update event (used only with Cinemotion Events).
		EM_CINEMOTION_UPDATE = 17,
		/// Trigger start cinemotion.
		EM_CINEMOTION_START = 18,
		/// Trigger stop cinemotion.
		EM_CINEMOTION_STOP = 19,
		/// Trigger preload cinemotion file for future use.
		EM_CINEMOTION_PRELOAD_FILE = 20,


		/// Internal value, do not use.
		EM_COUNT = 21
	};
}

#endif  // _DBOX_EEVENTMEANING_H_INCLUDED_
