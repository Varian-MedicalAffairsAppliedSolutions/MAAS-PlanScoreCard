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
* DVH view to better visualize Dose@Volume & Volume@Dose and other selectable metrics
* Dose per fraction and number of fractions stored in scorecard
  * Linear scaling of all doses on scorecard possible if # fx and dose/fx not matching Eclipse
* Checkbox to annotate note explaining specific metric performance (for report printing)
* CSV or PDF output reports
* Example ScoreCards can be found here: https://medicalaffairs.varian.com/dose-scorecards

### Main Screen
![image](https://user-images.githubusercontent.com/78000769/229181378-7cbc1207-08f6-4163-b270-1d1c0f9f0ebc.png)
1.	Checks ScoreCard Rx matches Rx in Eclipse: On Rx mismatch, option to scale all doses by the % difference
2.	Open previously saved ScoreCard and score currently selected plan(s)
3.	Create new ScoreCard or edit currently loaded ScoreCard
4.	Open additional patients/plan to additionally score in batch (NEW in v3.X)
5.	Open all courses and plans for selected patient
6.	Normalize dose in primary selected plan to maximum score
7.	Dump values in current ScoreCard evaluation view to CSV (Pandas Dataframe)
8.	Render current ScoreCard evaluation view to PDF for printing/reporting
9.	Solid selected box indicates primary plan selected
10.	Warnings and flags when a metric is below variation or scores 0 points
11.	Indication that a plan structure has been matched to a scorecard structure
12. Simplified DVH view of selectable scorecard metrics

### ScoreCard Editor
![image](https://user-images.githubusercontent.com/78000769/210690412-a6f01bfd-5975-4008-ac33-5bf57c9dcecb.png)
1.	Add a new metric to score
2.	Delete selected score metric
3.	Copy selected score metric
4.	Opens structure builder
5.	Moves the rank number (up or down) of selected metric
6.	Add a new point to the selected score metric
7.	Delete selected score point on selected score metric
8.	Order metric points in order of score value (highest to lowest)
9.	Clears colors and labels from all metric points
10.	 Moves the rank number (up or down) of the selected metric point
11.	Pushes the plan structure ID to the scorecard structure ID for the selected metric
12.	Opens the structure dictionary
13.	Select metric point that begins the “variation acceptable” range (yellow line) of the piecewise linear function
14.	Add colors and labels for each metric point
15.	Select treatment site (options can be edited in config file)
16.	Recalculates the plan score and returns to the main screen
17.	Save changes to json file

### Structure Builder
![image](https://user-images.githubusercontent.com/78000769/210690982-01ea862b-328b-4a80-a538-42d39de1d89e.png)
1.	Add a new structure grouping
2.	Add an operation step to the selected structure group
3.	Delete operation step
4.	Edit the operation steps of a selected group
5.	Select the Boolean operation between two intragroup steps
6.	Delete the selected group (along with all its operations)
7.	Choose the Boolean operation between different groups
8.	Demote/promote the selected group order sequence
9.	Free text field for the generated structure name
10.	Creates the plan structure in the metric editor. Note: The structure is not generated until it is assigned to a score metric in the Scorecard editor and a plan is scored with that scorecard.

### DVH View
![image](https://user-images.githubusercontent.com/78000769/229590966-85f77072-5f55-4d25-89d6-4934557ea475.png)

### Installation Guide: [Quick-Start](../master/BasicInstallQuickStart.md)

### Installation Guide Part2: [Move PlanScoreCard to system scripts directory, copy and edit launcher (optional)](../master/InstallGuidePart2IntoSystemScriptsDirectory.md)

### Errors when launching? See: [Troubleshooting](../master/Troubleshooting.md)

### Still have questions? See: [FrequentlyAskedQuestions](../master/FAQ.md)

## Current version: 3.1.7.X (4/3/2023)

### Improvements

* DVH rendering popout UI with horizontal(D@V)+vertical(V@D) scoring brackets per selected structure
* User can now select multiple metrics using CTRL/Shift in the editor 
* Normalization to max score range is now configurable and “index” type metrics can be turned off during normalization 
* Negative scores are no longer plotted by default for normalization unless PlotNegative is “true” in config file

### Bugfixes: 

* Fixed error where a crash occurs if the Rx dose is scaled too low and a conformation number metric is used
* Normalizing with a scorecard that has a HI metric will no longer cause the application to crash 
* Score values on Y-axis after plan normalization are now correctly displayed
* Dose precision is rounded to the nearest hundredth value when rescaling 
* Adding a point before selecting a metric will no longer cause the application to crash 
* Truncated printed metric text numerical values 
* If a null RX is input into the scorecard, the scaling button is no longer available 
* Input value now updates when changing the metric type of an existing metric  
* Patient selection view size was increased 
* Removing selected patient during batch scoring no longer causes the application to crash 
* Batch scoring now loads saved plan selections 
* Validation table no longer closes after making a structure match in batch scoring 
* The variation range dynamically changes in the editor 
* Fixed a bug where opening all plans and courses could not be selected after normalizing to max score 

### Known issues: 

* sometimes plot for normalizing to max score is not displayed in real time and only the final plot is shown
* Normalizing with a modified gradient metric causes score discrepancies between the pluginview and final plan 
* Prevent single point metrics from being saved as a template or recalculated 
* For a new structure dictionary entry, the user must click outside of the “Key” text box for the “OK” button to be enable 

### For changes in previous versions see: [ChangeLog](../master/ChangeLog.md)

### 3.X Feature roadmap: 
* BatchMode re-optimize multiple plans with RapidPlan model
*	BatchMode normalize multiple plans to max score
*	Guided structure builder (aka: simple mode, to be in front of current advanced structure builder)
*	Include support for asymmetric expansions in the advanced structure builder
*	Command line interface to output plan scores to the CLI or automate generation of CSV/PDF
* Add stylized html report output option (in addtion to clean CSV and viewdump PDF)
* DICOM Filemode support to be enabled by importing opensource C# DICOM library
*	Import/Export buttons on ScoreCard Builder to support opening and saving various file types
*	Reload current patient from Eclipse button
* API interface

