# PlanScoreCard

##### Medical Affairs Applied Solutions ESAPI tool to create dosimetric ScoreCards and score plans.
##### Features:
* Quantitative peicewise linear scoring functions for each metric
  * optional: flag for point where "variation acceptable" sited on referenced protocol
  * optional: note section to site referenced protocol or justification for metric (points)
  * optional: qualititive colors and labels for metric points, ie: orange="Just OK"
* Compare multiple plans in multiple courses at the same time for a given patient
* Advanced scoring critera supported
* Advanced structure matching logic with integrated customizable preloaded json stucture dictionary
* In-metric Structure Boolean/expansions created dynamically for scoring 
  * optionally save back to Plan if script approval/writable
* Normalize dose to max score
  * removes noise during plan comparison from suboptimal normalization impacting relative scores
  * re-normalized to max score plans saved into a new course
* Dose per fraction and number of fractions stored in scorecard
  * Linear scaling of all doses on scorecard possible if # fx and dose/fx not matching Eclipse
* Checkbox to annotate note explaining specific metric performance (for report printing)
* CSV or PDF output reports

### Main Screen
![MainScreen1 0 0](https://user-images.githubusercontent.com/78000769/169741084-0bb83fdc-69f4-4240-a193-e1e41db9c0df.png)

### ScoreCard Editor
![ScoreCardEditor1 0 0](https://user-images.githubusercontent.com/78000769/169741156-a27d6165-616a-4181-80a7-74ed7923e8eb.png)

### Structure Builder
![StructureBuilder1 0 0](https://user-images.githubusercontent.com/78000769/169741212-82f16a0d-1e11-4ee4-863d-ac2446486c5b.png)


## V1.0.0-05.19.2022
#### Improvements:
![MainScreenChanges](https://user-images.githubusercontent.com/78000769/169741261-04f08e03-1d04-47dd-a3e4-93f7f0198f6a.png)
* MainScreen UI improvements:
A. Not Validated for Clinical USE moved to title.
B. The word Summary Removed
C. Summary moved to top of application.
D. Flags and Warnings turned into buttons and don't grow with the number of messages anymore.
![warning-errors](https://user-images.githubusercontent.com/78000769/169742259-6bd0ff3d-3586-4c9e-92d7-b4801224459e.png)
E. The word "Plan Scores:" is no longer on its own line.
F. Unit put on value.
G. EULA moved under Plan Selection.
H. Open Patient button removed (it didn't do anything anyway)
I. Patient selection textbox disable from editing.
J. Score Rx scaling disappears if PRIMARY plan Rx matches the scorecard
* ScoreCard Builder UI improvements
  * 
* Structure Builder improvements
  * Operations between non-HighRes and HighRes solved (low-res converted to high-res first)
    
	
	Total Dose now calculated from dose per fraction and number of fractions, but is not modifyable in itself.
#### Bugfixes:
* Fixed issue where scorecard was crashing on modified gradient index.
  * This was crashing because the structure was left empty when building the scorecard. Now if the structure is null, then the scorecard will not crash, but going forward if the user tries to build a scorecard and doesn't select the structure than it will not let the user save.
  * Please choose body when selecting this metric if the metric is to be calculated in the entire body
* Fixed CI throwing error
* Fixed HI defaulting to cGy when not selected
* Set the structure collection to clear each time so it should only show the primary plan's structures.
* Can now delete metric comment all the way
  
#### Known issues
* Unable to change metric type or edit metrics after clicking recalulate score (must restart PlanScoreCard and reload to edit)
* No longer real time plot updating in Metric Editor
* Intermittent color change on adjacent label in qualitative label/edit color picker
* Certain patients cause crash on launching PlanScoreCard (only seen in V17)
 
#### Feature roadmap
* guided structure builder (aka: simple mode, to be in front of current advanced structure builder)
* reload current patient from Eclipse button
* command line interface
* optional DVH rendering below UI with horizontal(D@V)+vertical(V@D) brackets per selected stucture
