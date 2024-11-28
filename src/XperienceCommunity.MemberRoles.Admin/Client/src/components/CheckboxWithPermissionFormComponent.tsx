import { FormComponentProps, useFormComponentCommand, useFormComponentCommandProvider, usePageCommand } from "@kentico/xperience-admin-base";
import { Button, ButtonColor, ButtonSize, Checkbox, IconName, LinkButton, NotificationBarInfo } from "@kentico/xperience-admin-components";
import React, { useState } from "react";
import { MemberRolesPermissionSummary, MemberRolesPermissionSummaryComponent } from "./MemberRolesPermissionSummaryComponent";
import { MemberRoleConstants } from "../helpers/MemberRoleConstants";
import { BrowserDataHelper } from "../helpers/BrowserDataHelper";
const Commands = {
    RefreshPermissions: "RefreshPermissions",
};
export const CheckboxWithPermissionFormComponent = (props: CheckboxWithMemberRoleSummaryClientProperties) => {
    const { executeCommand } = useFormComponentCommandProvider();

    const [permissionSummary, setPermissionSummary] = useState<MemberRolesPermissionSummary>(props.memberRolePermissionSummary);
    const [infoDismissed, setInfoDismissed] = useState((BrowserDataHelper.getLocalStorage(MemberRoleConstants._ContentItemInfoDismissedKey) ??  "false") === "true");

    const handleOnChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (props.onChange) {
            props.onChange(e.target.checked);
        }
    };

    const refreshPermissions = async () => {
        const result = await executeCommand<MemberRolesPermissionSummary, void>(props, "RefreshPermissions");
        if(result) {
            setPermissionSummary(result);
        }
    }
    var dismissInfo = function() {
        BrowserDataHelper.setLocalStorage(MemberRoleConstants._ContentItemInfoDismissedKey, "true", 30);
        setInfoDismissed(true);
    }
    return (
        <>
        {!infoDismissed && 
                            <>
                                <NotificationBarInfo onDismiss={dismissInfo} childrenAsHtml={true}>
                                    <div>
                                These security settings are only applicable to this specific language instance, are governed by workflows and versioning, and are not inheritable by any other content item.<br/><br/>
                                To create hierarchy permission inheritances, use either the <strong>Properties - Member Permissions</strong> (Web Pages), or use the <strong>Folder Permissions</strong> Application (Content Hub).<br/><br/>
                                The Summary is not automatically refreshed on save.  After saving, click the <strong>Refresh Permission Summary</strong> to get an updated summary for if the changes are published on this item.
                                </div>
                                </NotificationBarInfo><br/>
                            </>
                        }
        <Button label={"Refresh Permission Summary"} title={"Click this after saving to refresh."} color={ButtonColor.Quinary} size={ButtonSize.XS} trailingIcon="xp-rotate-double-right" onClick={async () => await refreshPermissions()}/>
        <MemberRolesPermissionSummaryComponent 
            inheritingFrom={permissionSummary.inheritingFrom}
            requiresAuthentication={permissionSummary.requiresAuthentication}
            memberRoles={permissionSummary.memberRoles} 
            editLink={permissionSummary.editLink}/>

        <Checkbox
            name={props.name}
            label={props.label}
            onChange={handleOnChange}
            checked={props.value}
            disabled={props.disabled}
            invalid={props.invalid}
            validationMessage={props.validationMessage}
            markAsRequired={props.required}
            inactiveMessage={props.inactiveMessage}
            labelIcon={getFormLabelIcon(props.tooltip)}
            labelIconTooltip={props.tooltip}
            explanationText={props.explanationText}
            tooltipAsHtml={props.tooltipAsHtml}
            explanationTextAsHtml={props.explanationTextAsHtml}
        />
        </>
    );

};
export const getFormLabelIcon = (toolTipText: string | undefined): IconName | undefined => {
    return toolTipText ? 'xp-i-circle' : undefined;
};

interface CheckboxWithMemberRoleSummaryClientProperties extends FormComponentProps {
    memberRolePermissionSummary : MemberRolesPermissionSummary
}