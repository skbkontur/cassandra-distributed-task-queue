// @flow
import React from 'react';
import _ from 'lodash';

export default function customRender(target: any, path: string[]): React.Element<*> | null {
    if (_.isEqual(path, ['transportBox', 'id'])) {
        return (
            <a
                data-tid='GoToLink'
                href={'/AdminTools/TablesView/TransportBoxStorageElement/' +
                    `${target.transportBox.id}/${target.transportBox.id}`}>
                {target.transportBox.id}
            </a>
        );
    }

    if (_.isEqual(path, ['transportBox', 'primaryKey', 'forBoxId'])) {
        return (
            <a
                data-tid='GoToLink'
                href={'/AdminTools/TablesView/BoxStorageElement/' +
                    `${target.transportBox.primaryKey.forBoxId}/${target.transportBox.primaryKey.forBoxId}`}>
                {target.transportBox.primaryKey.forBoxId}
            </a>
        );
    }

    if (_.isEqual(path, ['transportBox', 'partyId'])) {
        return (
            <a
                data-tid='GoToLink'
                href={'/AdminTools/TablesView/Party2/' +
                    `${target.transportBox.partyId}/${target.transportBox.partyId}`}>
                {target.transportBox.partyId}
            </a>
        );
    }

    if (_.isEqual(path, ['computedConnectorInteractionId'])) {
        return (
            <a
                data-tid='GoToLink'
                href={'/AdminTools/TablesView/ConnectorInteractionContextStorageElement/' +
                    `${target.computedConnectorBoxId}/${target.computedConnectorInteractionId}`}>
                {target.computedConnectorInteractionId}
            </a>
        );
    }

    if (_.isEqual(path, ['fullDiadocPackageIdentifiers', 'invoiceEntityId'])) {
        const id = target.fullDiadocPackageIdentifiers.boxId;
        const letterId = target.fullDiadocPackageIdentifiers.messageId;
        const documentId = target.fullDiadocPackageIdentifiers.invoiceEntityId;
        return (
            <a
                data-tid='GoToLink'
                href={`https://diadoc.kontur.ru/${id}/Document/Show?letterId=${letterId}&documentId=${documentId}`}>
                {documentId}
            </a>
        );
    }

    if (_.isEqual(path, ['fullDiadocPackageIdentifiers', 'invoiceCorrectionEntityId'])) {
        const id = target.fullDiadocPackageIdentifiers.boxId;
        const letterId = target.fullDiadocPackageIdentifiers.messageId;
        const documentId = target.fullDiadocPackageIdentifiers.invoiceCorrectionEntityId;
        return (
            <a
                data-tid='GoToLink'
                href={`https://diadoc.kontur.ru/${id}/Document/Show?letterId=${letterId}&documentId=${documentId}`}>
                {documentId}
            </a>
        );
    }

    if (_.isEqual(path, ['fullDiadocPackageIdentifiers', 'torg12EntityId'])) {
        const id = target.fullDiadocPackageIdentifiers.boxId;
        const letterId = target.fullDiadocPackageIdentifiers.messageId;
        const documentId = target.fullDiadocPackageIdentifiers.torg12EntityId;
        return (
            <a
                data-tid='GoToLink'
                href={`https://diadoc.kontur.ru/${id}/Document/Show?letterId=${letterId}&documentId=${documentId}`}>
                {documentId}
            </a>
        );
    }

    if (_.isEqual(path, ['fullDiadocPackageIdentifiers', 'universalTranferDocumentEntityId'])) {
        const id = target.fullDiadocPackageIdentifiers.boxId;
        const letterId = target.fullDiadocPackageIdentifiers.messageId;
        const documentId = target.fullDiadocPackageIdentifiers.universalTranferDocumentEntityId;
        return (
            <a
                data-tid='GoToLink'
                href={`https://diadoc.kontur.ru/${id}/Document/Show?letterId=${letterId}&documentId=${documentId}`}>
                {documentId}
            </a>
        );
    }

    return null;
}
