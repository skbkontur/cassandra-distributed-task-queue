// @flow
import React from 'react';
import { Icon, Link, LinkDropdown } from 'ui';
import _ from 'lodash';
const LinkMenuItem = LinkDropdown.MenuItem;

const endsWith = (ending: string, str: string): boolean => str.slice(-ending.length) === ending;

// eslint-disable-next-line flowtype/no-weak-types
function getByPath(target: ?Object, path: string[]): mixed {
    // @flow-coverage-ignore-next-line
    return _.get(target, path);
}

// eslint-disable-next-line max-statements
export default function customRender(target: mixed, path: string[]): React.Element<any> | null {
    const pathTop = path[path.length - 1];
    if (target == null) {
        return null;
    }
    if (endsWith('PartyId', pathTop) && typeof target === 'object') {
        const partyId = getByPath(target, path);
        if (typeof partyId === 'string') {
            return (
                <Link data-tid='GoToLink' href={`/AdminTools/PartyEdit?partyId=${partyId}`}>
                    {partyId}
                </Link>
            );
        }
    }

    if (
        (pathTop === 'connectorBoxId' || endsWith('connectorBoxId', pathTop) || endsWith('ConnectorBoxId', pathTop)) &&
        typeof target === 'object'
    ) {
        const connectorBoxId = getByPath(target, path);
        if (typeof connectorBoxId === 'string') {
            return (
                <Link
                    data-tid='GoToLink'
                    href={`/AdminTools/TablesView/ConnectorBoxStorageElement/${connectorBoxId}/${connectorBoxId}`}>
                    {connectorBoxId}
                </Link>
            );
        }
    }

    if ((pathTop === 'boxId' || endsWith('BoxId', pathTop)) && typeof target === 'object') {
        const boxId = getByPath(target, path);
        if (typeof boxId === 'string') {
            return (
                <Link data-tid='GoToLink' href={`/AdminTools/TablesView/BoxStorageElement/${boxId}/${boxId}`}>
                    {boxId}
                </Link>
            );
        }
    }

    if (pathTop === 'id' && typeof target === 'object') {
        const id = getByPath(target, path);
        if (typeof id === 'string') {
            if (['deliveryBox', 'transportBox'].includes(path[path.length - 2])) {
                return (
                    <Link data-tid='GoToLink' href={`/AdminTools/TablesView/TransportBoxStorageElement/${id}/${id}`}>
                        {id}
                    </Link>
                );
            }

            if (['inbox', 'outbox', 'box'].includes(path[path.length - 2])) {
                return (
                    <Link data-tid='GoToLink' href={`/AdminTools/TablesView/BoxStorageElement/${id}/${id}`}>
                        {id}
                    </Link>
                );
            }
        }
    }

    if (pathTop === 'partyId' && typeof target === 'object') {
        const partyId = getByPath(target, path);
        if (typeof partyId === 'string') {
            return (
                <Link data-tid='GoToLink' href={`/AdminTools/TablesView/Party2/${partyId}/${partyId}`}>
                    {partyId}
                </Link>
            );
        }
    }

    if (_.isEqual(path, ['computedConnectorInteractionId']) && typeof target === 'object') {
        if (
            typeof target.computedConnectorBoxId === 'string' &&
            typeof target.computedConnectorInteractionId === 'string'
        ) {
            return (
                <Link
                    data-tid='GoToLink'
                    href={
                        '/AdminTools/TablesView/ConnectorInteractionContextStorageElement/' +
                        `${target.computedConnectorBoxId}/${target.computedConnectorInteractionId}`
                    }>
                    {target.computedConnectorInteractionId}
                </Link>
            );
        }
    }

    if (_.isEqual(path, ['fullDiadocPackageIdentifiers', 'invoiceEntityId']) && typeof target === 'object') {
        if (target.fullDiadocPackageIdentifiers != null && typeof target.fullDiadocPackageIdentifiers === 'object') {
            const id = target.fullDiadocPackageIdentifiers.boxId;
            const letterId = target.fullDiadocPackageIdentifiers.messageId;
            const documentId = target.fullDiadocPackageIdentifiers.invoiceEntityId;
            if (typeof id === 'string' && typeof letterId === 'string' && typeof documentId === 'string') {
                return (
                    <Link
                        data-tid='GoToLink'
                        href={
                            `https://diadoc.kontur.ru/${id}/Document/Show?letterId=${letterId}&` +
                            `documentId=${documentId}`
                        }>
                        {documentId}
                    </Link>
                );
            }
        }
    }

    if (_.isEqual(path, ['fullDiadocPackageIdentifiers', 'invoiceCorrectionEntityId']) && typeof target === 'object') {
        if (target.fullDiadocPackageIdentifiers != null && typeof target.fullDiadocPackageIdentifiers === 'object') {
            const id = target.fullDiadocPackageIdentifiers.boxId;
            const letterId = target.fullDiadocPackageIdentifiers.messageId;
            const documentId = target.fullDiadocPackageIdentifiers.invoiceCorrectionEntityId;
            if (typeof id === 'string' && typeof letterId === 'string' && typeof documentId === 'string') {
                return (
                    <Link
                        data-tid='GoToLink'
                        href={
                            `https://diadoc.kontur.ru/${id}/Document/Show?letterId=${letterId}&` +
                            `documentId=${documentId}`
                        }>
                        {documentId}
                    </Link>
                );
            }
        }
    }

    if (_.isEqual(path, ['fullDiadocPackageIdentifiers', 'torg12EntityId']) && typeof target === 'object') {
        if (target.fullDiadocPackageIdentifiers != null && typeof target.fullDiadocPackageIdentifiers === 'object') {
            const id = target.fullDiadocPackageIdentifiers.boxId;
            const letterId = target.fullDiadocPackageIdentifiers.messageId;
            const documentId = target.fullDiadocPackageIdentifiers.torg12EntityId;
            if (typeof id === 'string' && typeof letterId === 'string' && typeof documentId === 'string') {
                return (
                    <Link
                        data-tid='GoToLink'
                        href={
                            `https://diadoc.kontur.ru/${id}/Document/Show?letterId=${letterId}&` +
                            `documentId=${documentId}`
                        }>
                        {documentId}
                    </Link>
                );
            }
        }
    }

    if (
        _.isEqual(path, ['fullDiadocPackageIdentifiers', 'universalTranferDocumentEntityId']) &&
        typeof target === 'object'
    ) {
        if (target.fullDiadocPackageIdentifiers != null && typeof target.fullDiadocPackageIdentifiers === 'object') {
            const id = target.fullDiadocPackageIdentifiers.boxId;
            const letterId = target.fullDiadocPackageIdentifiers.messageId;
            const documentId = target.fullDiadocPackageIdentifiers.universalTranferDocumentEntityId;
            if (typeof id === 'string' && typeof letterId === 'string' && typeof documentId === 'string') {
                return (
                    <Link
                        data-tid='GoToLink'
                        href={
                            `https://diadoc.kontur.ru/${id}/Document/Show?letterId=${letterId}&` +
                            `documentId=${documentId}`
                        }>
                        {documentId}
                    </Link>
                );
            }
        }
    }

    if (_.isEqual(path, ['documentCirculationId']) && typeof target === 'object') {
        if (typeof target.documentCirculationId === 'string') {
            return (
                <Link data-tid='GoToLink' href={`/Monitoring/AdminTaskChainDetails?id=${target.documentCirculationId}`}>
                    {target.documentCirculationId}
                </Link>
            );
        }
    }

    if (pathTop === 'rawMessageId' && typeof target === 'object') {
        const rawMessageId = getByPath(target, path);
        const transportBoxId = getByPath(target, [...path.slice(0, path.length - 1), 'transportBox', 'id']);
        if (typeof rawMessageId === 'string' && typeof transportBoxId === 'string') {
            return (
                <LinkDropdown renderTitle={rawMessageId}>
                    <LinkMenuItem
                        href={`/AdminTools/TablesView/RawMessageMetaInformation/${transportBoxId}/${rawMessageId}`}>
                        <Icon name='doc-o' /> Открыть RawMessageMetaInformation
                    </LinkMenuItem>
                    <LinkMenuItem
                        href={`/AdminTools/FileData?fileId=${transportBoxId}_${rawMessageId}`}>
                        <Icon name='download' /> Скачать файл
                    </LinkMenuItem>
                </LinkDropdown>
            );
        }
    }

    return null;
}
