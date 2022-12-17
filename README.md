# PlanScoreCard

##### Medical Affairs Applied Solutions ESAPI tool to create dosimetric ScoreCards and score plans.
##### Features:
* Quantitative piecewise linear scoring functions for each metric
  * optional: flag for point where "variation acceptable" sited on referenced protocol
  * optional: note section to site referenced protocol or justification for metric (points)
  * optional: qualitative colors and labels for metric points, ie: orange="Just OK"
* Advanced scoring criteria supported

  ![image](https://user-images.githubusercontent.com/78000769/208264370-51b853f4-59dd-498a-8dd7-17b093d0e6f2.png)
  
* Compare multiple plans in multiple courses at the same time for a given patient
* Compare plans on multiple patients simultaniously in BatchMode (NEW in v3.X)
  * save patient/plan batch list to .json file for subsequent re-evalution of a scorecard on that list
* Advanced structure matching logic with integrated customizable preloaded json structure dictionary
* In-metric Structure Boolean/expansions created dynamically for scoring 
  * optionally save back to Plan if script approval/writable (to auto-create optimization tuning structures)
* Normalize dose to max score
  * removes noise during plan comparison from suboptimal normalization impacting relative scores
  * re-normalized to max score plans saved into a new course
* Dose per fraction and number of fractions stored in scorecard
  * Linear scaling of all doses on scorecard possible if # fx and dose/fx not matching Eclipse
* Checkbox to annotate note explaining specific metric performance (for report printing)
* CSV or PDF output reports
* Example ScoreCards can be found here: https://medicalaffairs.varian.com/dose-scorecards

### Main Screen
![MainScreen1 0 0](https://user-images.githubusercontent.com/78000769/200408043-41a14e05-d9e3-4cad-99a0-442bf8e6a654.png)
1) Checks ScoreCard Rx matches Rx in Eclipse: On Rx mismatch, option to scale all doses by the % difference
2) Open previously saved ScoreCard and score currently selected plan(s)
3) Create new ScoreCard or edit currently loaded ScoreCard
4) Open additional patients/plan to additionally score in batch (NEW in v3.X)
5) Normalize dose in primary selected plan to maxiumum score
6) Dump values in current ScoreCard evaluation view to CSV (Pandas Dataframe)
7) Render current ScoreCard evaluation view to PDF for printing/reporting
8) Solid selected box indicates primary plan selected 

### ScoreCard Editor
![ScoreCardEditor1 0 0](https://user-images.githubusercontent.com/78000769/169741156-a27d6165-616a-4181-80a7-74ed7923e8eb.png)

### Structure Builder
![StructureBuilder1 0 0](https://user-images.githubusercontent.com/78000769/169741212-82f16a0d-1e11-4ee4-863d-ac2446486c5b.png)

### Installation Guide: [Quick-Start](../master/BasicInstallQuickStart.md)

### Installation Guide Part2: [Move PlanScoreCard to system scripts directory, copy and edit launcher (optional)](../master/InstallGuidePart2IntoSystemScriptsDirectory.md)

### Errors when launching? See: [Troubleshooting](../master/Troubleshooting.md)

### Still have questions? See: [FrequentlyAskedQuestions](../master/FAQ.md)

## Current version: 3.0.3.X (11/08/2022)
### Improvements
* Batch mode: CSV input with multiple patients/plans and report output 
![image](https://user-images.githubusercontent.com/78000769/208266047-331154ea-946f-4e5f-9597-7f3a20297e93.png)
![image](https://user-images.githubusercontent.com/78000769/208266064-b38b04d9-f2d6-4797-8017-e54d4b691f6b.png)
* Patient ID added to scorecard warnings and flags 
![image](https://user-images.githubusercontent.com/78000769/208266072-672c2a4f-03d2-4552-b4d6-988bdfe5ded8.png)
* Implement point falloff logic (tails) once a metric fails (zero points) when normalizing to max score. Tail values can be adjusted in the “PlanScoreCard.exe” config file.
![image](https://user-images.githubusercontent.com/78000769/208266421-e0f8ca06-cb41-474a-9c87-ea2559cbc623.png)
![image](https://user-images.githubusercontent.com/78000769/208266434-d454fbb3-784c-45f3-9013-9d02149aaa41.png)


### Bugfixes: 

* Invalid null structures can no longer be added to the structure dictionary 
* All unselected plan scores are now cleared in the last column on the following events: rx scaling, scorecard editing, and loading a new scorecard 
* Volume precision is rounded to the nearest hundredth value when rescaling. 
* Rescaling now correctly applies to HI metric 
* Color selection option was changed from “Cancel” to “Clear” 

### Known issues: 

* Prevent single point metrics from being saved as a template or recalculated. 
* A crash occurs if the Rx dose is scaled too low and the conformation number metric is used. 
* Score values on Y-axis during plan normalization are incorrect and sometime tails are wrongly created: normalize to max score not 100% reliable
* Normalizing with a scorecard that has a HI metric will cause the application to crash 
* Adding a point before selecting a metric will cause the application to crash 

### 3.X Feature roadmap: 
* BatchMode re-optimize multiple plans with RapidPlan model
*	BatchMode normalize multiple plans to max score
* DVH view button for popout UI with horizontal(D@V)+vertical(V@D) scoring brackets per selected structure
*	Guided structure builder (aka: simple mode, to be in front of current advanced structure builder)
*	Include support for asymmetric expansions in the advanced structure builder
*	Command line interface to output plan scores to the CLI or automate generation of CSV/PDF
* DICOM Filemode support to be enabled by importing opensource C# DICOM library
*	Import/Export buttons on ScoreCard Builder to support opening and saving various file types
*	Reload current patient from Eclipse button
* API interface

