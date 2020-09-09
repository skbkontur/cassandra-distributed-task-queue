import Link from "@skbkontur/react-ui/Link";
import React from "react";

import { CommonLayout } from "../Layouts/CommonLayout";

import SorryImage from "./Sorry.png";
import styles from "./TaskNotFoundPage.less";

interface TaskNotFoundPageProps {
    parentLocation: string;
}

export class TaskNotFoundPage extends React.Component<TaskNotFoundPageProps> {
    public render(): JSX.Element {
        const { parentLocation } = this.props;

        return (
            <CommonLayout>
                <CommonLayout.GoBack to={parentLocation}>Вернуться к поиску задач</CommonLayout.GoBack>
                <CommonLayout.Header title="По этому адресу задачи нет" />
                <CommonLayout.Content>
                    Это могло произойти по двум причинам:
                    <ul className={styles.list}>
                        <li>Неправильный URL. Возможно вы не полностью скопировали ссылку.</li>
                        <li>
                            Задача удалена, потому что она старше 200 дней. Если вам нужна информация из этой задачи,
                            напишите дежурному аналитику на{" "}
                            <Link href="mailto:ask.edi@skbkontur.ru">ask.edi@skbkontur.ru</Link>.
                        </li>
                    </ul>
                    <img className={styles.sorryImage} width={1190} height={515} src={SorryImage} />
                </CommonLayout.Content>
            </CommonLayout>
        );
    }
}
