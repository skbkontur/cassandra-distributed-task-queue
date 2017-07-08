// @flow
import React from 'react';
import { Link } from 'ui';
import CommonLayout, {
    CommonLayoutGoBack,
    CommonLayoutHeader,
    CommonLayoutContent,
} from '../../../Commons/Layouts';
import type { RouterLocationDescriptor } from 'react-router';
import SorryImage from './Sorry.png';
import cn from './TaskNotFoundPage.less';

type TaskNotFoundPageProps = {
    parentLocation: RouterLocationDescriptor;
};

export default class TaskNotFoundPage extends React.Component {
    props: TaskNotFoundPageProps;

    render(): React.Element<*> {
        const { parentLocation } = this.props;

        return (
            <CommonLayout>
                <CommonLayoutGoBack to={parentLocation}>
                    Вернуться к поиску задач
                </CommonLayoutGoBack>
                <CommonLayoutHeader title='По этому адресу задачи нет' />
                <CommonLayoutContent>
                    Это могло произойти по двум причинам:
                    <ul className={cn('list')}>
                        <li>Непроавильный URL. Возможно вы не полностью скопировали ссылку.</li>
                        <li>
                            Задача удалена, потому что она старше 200 дней. Если вам нужна информация из этой задачи,
                            напишите дежурному аналитику на <Link href='mailto:ask.edi@skbkontur.ru'>ask.edi@skbkontur.ru</Link>
                        </li>
                    </ul>
                    <img className={cn('sorry-image')} width={1190} height={515} src={SorryImage} />
                </CommonLayoutContent>
            </CommonLayout>
        );
    }
}
