using System;
using System.Workflow.Activities;
using Composite.C1Console.Actions;
using Composite.C1Console.Events;
using Composite.Data;
using Composite.Data.Types;
using Composite.Core.ResourceSystem;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.Logging;


namespace Composite.Plugins.Elements.ElementProviders.UserElementProvider
{
    [EntityTokenLock()]
    [AllowPersistingWorkflow(WorkflowPersistingType.Idle)]
    public sealed partial class DeleteUserWorkflow : Composite.C1Console.Workflow.Activities.FormsWorkflow
    {
        private bool _deleteSelf = false;



        public DeleteUserWorkflow()
        {
            InitializeComponent();
        }



        private void IsDeleteSelf(object sender, ConditionalEventArgs e)
        {
            e.Result = _deleteSelf;
        }



        private void initializeCodeActivity_Initialize_ExecuteCode(object sender, EventArgs e)
        {
            DataEntityToken dataEntityToken = (DataEntityToken)this.EntityToken;

            IUser user = (IUser)dataEntityToken.Data;

            _deleteSelf = user.Username == UserValidationFacade.GetUsername();
        }



        private void finalizeCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            DeleteTreeRefresher deleteTreeRefresher = this.CreateDeleteTreeRefresher(this.EntityToken);

            DataEntityToken dataEntityToken = (DataEntityToken)this.EntityToken;
            
            IUser user = (IUser)dataEntityToken.Data;

            if (DataFacade.WillDeleteSucceed(user))
            {
                UserPerspectiveFacade.DeleteAll(user.Username);

                DataFacade.Delete(user);

                LoggingService.LogVerbose("UserManagement", String.Format("C1 Console user '{0}' deleted by '{1}'.", user.Username, UserValidationFacade.GetUsername()), LoggingService.Category.Audit);

                deleteTreeRefresher.PostRefreshMesseges();
            }
            else
            {
                this.ShowMessage(
                        DialogType.Error,
                        StringResourceSystemFacade.GetString("Composite.Management", "DeleteUserWorkflow.CascadeDeleteErrorTitle"),
                        StringResourceSystemFacade.GetString("Composite.Management", "DeleteUserWorkflow.CascadeDeleteErrorMessage")
                    );
            }
        }        
    }
}
