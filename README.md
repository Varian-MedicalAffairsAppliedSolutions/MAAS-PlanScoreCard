# PlanScoreCard

##### Medical Affairs Applied Solutions ESAPI tool to create dosimetric ScoreCards and score plans.
##### Features:
* Quantitative peicewise linear scoring functions for each metric
  * optional: flag for point where "variaition acceptable" sited on referenced protocol
  * optional: note section to site referenced protocol or justification for metric (points)
  * optional: qualititive colors and labels for metric points, ie: orange="Needs Improvement"
* Compare multiple plans in multiple courses at the same time for a given patient
* Advanced scoring critera supported
* In-metric Structure Boolean/expansions created dynamically for scoring 
  * optionally save back to Plan if script approval/writable
* Normalize dose to max score
  * removes noise during plan comparison from suboptimal normalization impacting relative scores
  * re-normalized to max score plans saved into a new course
* CSV or PDF output reports

### Main Screen


## V1.0.0-05.19.2022
