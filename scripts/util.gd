class_name Util

## Time-independent damping with exponential-decay.
##
## [smoothing] is the proportion of [current] remaining after one second, the
## rest is [target]. [delta_time] represents the number of seconds passed since
## last invocation.
static func damp(current: Variant, target: Variant, smoothing: float, delta_time: float) -> Variant :
	# https://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
	return lerp(current, target, 1. - pow(smoothing, delta_time))
