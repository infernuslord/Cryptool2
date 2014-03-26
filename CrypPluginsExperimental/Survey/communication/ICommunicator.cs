using Survey.model;

namespace Survey.communication
{
    public interface ICommunicator {
        SurveyModel FetchSurvey();
        void PushSurvey(SurveyModel survey);
    }
}
