# Performance Tracing
A library to record performance traces in S&box games.

## Features
* Nanosecond resolution timings.
* Built-in Chrome trace event format.
* Embeddable custom meta data.
* Customizable trace storage and formatting.
* Three types of traces supported:
  * Performance - Records time taken to execute a block of code.
  * Counter - Records changes to a number for graphing. Useful for recording player counts and basic state.
  * Marker - Marks a point in a trace. Useful to track events happening in your code.

## Usage
See the [API wiki](https://github.com/peter-r-g/Sbox-PerformanceTracing/wiki)
 
## Installation
You can either download it from this repo or you can reference it with `gooman.perf_tracing` using [asset.party](https://asset.party/gooman/perf_tracing)

## License
Distributed under the MIT License. See the [license](https://github.com/peter-r-g/Sbox-PerformanceTracing/blob/master/LICENSE.md) for more information.
