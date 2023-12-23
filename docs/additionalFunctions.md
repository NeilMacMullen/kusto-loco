# Addition Functions

Kusto-loco supports a number of "non-standard" functions.  Note that these may be subject to change.

## levenshtein
Returns the [Levenshtein distance](https://en.wikipedia.org/wiki/Levenshtein_distance) between two strings.
This provides an approximate metric for considering how similar two strings are.  
Core functionality is provided using the [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) library by [Dan Harltey](https://github.com/DanHarltey)

## string_similarity
Returns a number from 0...1.0 indicating how similar two strings are.  This is calculated as 

`1.0 - levenshtein(a,b)/min(a.Length,b.Length)` 

- Two empty strings are considered to have a similarity of 1.
- Any non-empty string is considered to have a similarity of 0 with an empty string.

## datetime_to_iso
Formats a DateTime value as a string in [ISO8601](https://en.wikipedia.org/wiki/ISO_8601) format.