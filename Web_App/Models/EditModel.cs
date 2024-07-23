namespace Web_App.Models
{
    public class EditModel
    {
        public PatientModel PatientModel { get; set; }
        public TreatmentPlanModel TreatmentPlanModel { get; set; }
        public RemarkModel RemarkModel { get;set; }

        public EditModel(PatientModel patientModel, TreatmentPlanModel treatmentPlanModel, RemarkModel remarkModel)
        {
            PatientModel = patientModel;
            TreatmentPlanModel = treatmentPlanModel;
            RemarkModel = remarkModel;
        }
    }
}