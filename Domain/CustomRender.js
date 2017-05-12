// @flow
import React from 'react';
import _ from 'lodash';

const endsWith = (ending: string, str: string): boolean => str.slice(-ending.length) === ending;

// TODO эж, переписать бы функцию...
// eslint-disable-next-line max-statements
export default function customRender(target: any, path: string[]): React.Element<*> | null {
    const pathTop = path[path.length - 1];

    if (endsWith('PartyId', pathTop)) {
        const partyId = _.get(target, path);
        return (
            <a
                data-tid='GoToLink'
                href={`/AdminTools/PartyEdit?partyId=${partyId}`}>
                {partyId}
            </a>
        );
    }

    if (pathTop === 'connectorBoxId' || endsWith('connectorBoxId', pathTop) || endsWith('ConnectorBoxId', pathTop)) {
        const connectorBoxId = _.get(target, path);
        return (
            <a
                data-tid='GoToLink'
                href={`/AdminTools/TablesView/ConnectorBoxStorageElement/${connectorBoxId}/${connectorBoxId}`}>
                {connectorBoxId}
            </a>
        );
    }

    if (pathTop === 'boxId' || endsWith('BoxId', pathTop)) {
        const boxId = _.get(target, path);
        return (
            <a
                data-tid='GoToLink'
                href={`/AdminTools/TablesView/BoxStorageElement/${boxId}/${boxId}`}>
                {boxId}
            </a>
        );
    }

    if (pathTop === 'id') {
        const id = _.get(target, path);
        if (['deliveryBox', 'transportBox'].includes(path[path.length - 2])) {
            return (
                <a
                    data-tid='GoToLink'
                    href={`/AdminTools/TablesView/TransportBoxStorageElement/${id}/${id}`}>
                    {id}
                </a>
            );
        }

        if (['inbox', 'outbox', 'box'].includes(path[path.length - 2])) {
            return (
                <a
                    data-tid='GoToLink'
                    href={`/AdminTools/TablesView/BoxStorageElement/${id}/${id}`}>
                    {id}
                </a>
            );
        }
    }

    if (pathTop === 'partyId') {
        const partyId = _.get(target, path);
        return (
            <a
                data-tid='GoToLink'
                href={`/AdminTools/TablesView/Party2/${partyId}/${partyId}`}>
                {partyId}
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

    if (_.isEqual(path, ['documentCirculationId'])) {
        return (
            <a
                data-tid='GoToLink'
                href={`/Monitoring/AdminTaskChainDetails?id=${target.documentCirculationId}`}>
                {target.documentCirculationId}
            </a>
        );
    }

    return null;
}
