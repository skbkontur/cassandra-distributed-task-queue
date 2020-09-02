import { action } from "@storybook/addon-actions";
import * as React from "react";

import { TaskTypesSelect } from "../../src/RemoteTaskQueueMonitoring/components/TaskTypesSelect/TaskTypesSelect";

export default {
    title: "RemoteTaskQueueMonitoring/TaskTypesSelect",
    component: TaskTypesSelect,
};

export const Default = () => <TaskTypesSelect onChange={action("onChange")} availableTaskTypes={[]} value={[]} />;

export const WithSomeVariants = () => (
    <TaskTypesSelect
        value={[]}
        onChange={action("onChange")}
        availableTaskTypes={[
            "ASyncMdnSendTaskData",
            "Billing_CheckOrUpdatePortalRestrictionTaskData",
            "Billing_ExecuteCodeActivationTaskData",
            "Billing_InitActivationProcessTaskData",
            "Billing_MarkActivationProcessAsCompletedTaskData",
            "Billing_RollBackActivationProcessTaskData",
            "Billing_SetActivationCompletedTaskData",
            "Billing_SetProductionTrafficTaskData",
            "Billing_SetWaitingForActivationStatusTaskData",
            "Box_FinishPostInvoicToDiadocTaskData",
            "Box_MarkAsCheckingFailBoxTaskData",
            "Box_MarkAsCheckingOkBoxTaskData",
            "Box_MarkAsDeliveredBoxTaskData",
            "Box_MarkAsReadBoxTaskData",
            "Box_MarkAsReceiptConfirmationNotReceivedBoxTaskData",
            "Box_OutboxValidatingTaskData",
        ]}
    />
);

export const WithMoreVariants = () => (
    <TaskTypesSelect
        value={[]}
        onChange={action("onChange")}
        availableTaskTypes={[
            "ASyncMdnSendTaskData",
            "Billing_CheckOrUpdatePortalRestrictionTaskData",
            "Billing_ExecuteCodeActivationTaskData",
            "Billing_InitActivationProcessTaskData",
            "Billing_MarkActivationProcessAsCompletedTaskData",
            "Billing_RollBackActivationProcessTaskData",
            "Billing_SetActivationCompletedTaskData",
            "Billing_SetProductionTrafficTaskData",
            "Billing_SetWaitingForActivationStatusTaskData",
            "Box_FinishPostInvoicToDiadocTaskData",
            "Box_MarkAsCheckingFailBoxTaskData",
            "Box_MarkAsCheckingOkBoxTaskData",
            "Box_MarkAsDeliveredBoxTaskData",
            "Box_MarkAsReadBoxTaskData",
            "Box_MarkAsReceiptConfirmationNotReceivedBoxTaskData",
            "Box_OutboxValidatingTaskData",
            "Box_PostCoinvoicToDiadocTaskData",
            "Box_PostInvoicToDiadocTaskData",
            "Box_PropagatingDiadocRevocationAcceptedBoxTaskData",
            "Box_PropagatingDiadocRevocationAcceptedForBuyerBoxTaskData",
            "Box_PropagatingDraftOfDocumentDeletedFromDiadocBoxTaskData",
            "Box_PropagatingDraftOfDocumentSigningBySenderBoxTaskData",
            "Box_PropagatingDraftOfDocumentSigningBySenderForBuyerBoxTaskData",
            "Box_PropagatingPostedCleanIntoDiadocBoxTaskData",
            "Box_PropagatingPostedDraftIntoDiadocBoxTaskData",
            "Box_PropagatingReceivedDiadocRoamingErrorBoxTaskData",
            "Box_PropagatingRussianInvoiceReceiptByBuyerBoxTaskData",
            "Box_PropagatingTorg12RejectedSignByBuyerBoxTaskData",
            "Box_PropagatingTorg12SignedByBuyerBoxTaskData",
            "ClearStatusReportPotTaskData",
            "ConnectorBox_TakenToTransformationTaskData",
            "ConnectorBox_TransformationPausedTaskData",
            "ConnectorBox_TransformationResumedTaskData",
            "ConnectorBox_TransformedSuccessfullyTaskData",
            "ConnectorBox_TransformedUnsuccessfullyTaskData",
            "GenerateCheckReportTaskData",
            "MonitoringExportationTaskData",
            "NotifyAboutCompletedOrvisReportTaskData",
            "NotifyThroughAgentTaskData",
            "OrvisReportSchedulerTaskData",
            "PrintExcelTaskData",
            "ProcessAsyncMdnTaskData_New",
            "RequestOrvisReportTaskData",
            "SendEmailTaskData",
            "SynchronizePartyUsersToPortalTaskData",
            "SynchronizeUserPartiesToPortalTaskData",
            "TransportBox_ApiMessageDeliveryFinishedTaskData",
            "TransportBox_ApiMessageDeliveryStartedTaskData",
            "TransportBox_ApiSendMessageTaskData",
            "TransportBox_As2ResendIfNoMdnTaskData",
            "TransportBox_As2RunIfNoMdnTaskData",
            "TransportBox_As2SendMessageWithASyncMdnTaskData",
            "TransportBox_As2SendMessageWithSyncMdnTaskData",
            "TransportBox_DeliveryMessageTaskData",
            "TransportBox_DeliveryMessageToTransportBoxTaskData",
            "TransportBox_DispatchMessageTaskData",
            "TransportBox_LocalFtpSendMessageTaskData",
            "TransportBox_MarkAsCheckingFailTransportBoxTaskData",
            "TransportBox_MarkAsCheckingOkTransportBoxTaskData",
            "TransportBox_MarkAsDeliveredTransportBoxTaskData",
            "TransportBox_MarkAsFatalErrorTransportBoxTaskData",
            "TransportBox_MarkAsReadTransportBoxTaskData",
            "TransportBox_MarkAsReceiptConfirmationNotReceivedTransportBoxTaskData",
            "TransportBox_PropagatingDiadocRevocationAcceptedForBuyerTransportBoxTaskData",
            "TransportBox_PropagatingDiadocRevocationAcceptedTransportBoxTaskData",
            "TransportBox_PropagatingDraftOfDocumentDeletedFromDiadocTransportBoxTaskData",
            "TransportBox_PropagatingDraftOfDocumentSigningBySenderForBuyerTransportBoxTaskData",
            "TransportBox_PropagatingDraftOfDocumentSigningBySenderTransportBoxTaskData",
            "TransportBox_PropagatingPostedCleanIntoDiadocTransportBoxTaskData",
            "TransportBox_PropagatingPostedDraftIntoDiadocTransportBoxTaskData",
            "TransportBox_PropagatingReceivedDiadocRoamingErrorTransportBoxTaskData",
            "TransportBox_ProviderSendMessageTaskData",
            "TransportBox_RecognizeMessageTaskData",
            "TransportBox_RouteFromProviderTaskData",
            "TransportBox_SendMessageFromOutboxTaskData",
            "TransportBox_SplitMessageTaskData",
            "WaitBeforeDiadocRevocationAcceptedForBuyerTaskData",
            "WaitBeforeDiadocRevocationAcceptedTaskData",
            "WaitBeforeDiadocRoamingErrorTaskData",
            "WaitBeforeDraftOfDocumentSigningBySenderToReceiverTaskData",
            "WaitBeforeRussianInvoiceReceiptByBuyerTaskData",
            "WaitBeforeTorg12RejectedSignByBuyerTaskData",
        ]}
    />
);
