import React, { useState } from "react";
import { Box, Button, ButtonColor, Checkbox, CheckboxSize, Headline, HeadlineSize, Icon, Input, NotificationBarInfo, NotificationBarWarning, Paper, Spacing } from "@kentico/xperience-admin-components";
import { usePageCommand } from "@kentico/xperience-admin-base";
import { MemberRolesPermissionSummary, MemberRolesPermissionSummaryComponent } from "../components/MemberRolesPermissionSummaryComponent";
import { BrowserDataHelper } from "../helpers/BrowserDataHelper";
import { MemberRoleConstants } from "../helpers/MemberRoleConstants";
const Commands = {
    SetProperties: "SetProperties",
};

interface TagItem {
    tagTitle: string,
    tagName: string,
    tagId: number
}

interface ContentFolderMemberRoleProperties {
    breakInheritance : boolean,
    requireAuthentication: boolean,
    memberRoleTagIds: Array<number>
    availableRoles: Array<TagItem>
    memberRolePermissionSummary : MemberRolesPermissionSummary
}
interface ContentFolderMemberRoleData {
    breakInheritance : boolean,
    requireAuthentication: boolean,
    memberRoleTagIds: Array<number>
    memberRolePermissionSummary : MemberRolesPermissionSummary
}

export const ContentFolderMemberSecurityPageTemplate = ({ breakInheritance, requireAuthentication, memberRoleTagIds, availableRoles, memberRolePermissionSummary }: ContentFolderMemberRoleProperties) => {
    
    const [selectedRoleTagIds, setSelectedRoleTagIds] = useState(memberRoleTagIds);
    const [breakInheritanceVal, setBreakInheritance] = useState(breakInheritance);
    const [requireAuthenticationVal, setRequireAuthentication] = useState(requireAuthentication);
    const [warningDismissed, setWarningDismissed] = useState((BrowserDataHelper.getLocalStorage(MemberRoleConstants._NoWorkflowWarningDismissedKey) ?? "false") === "true");
    const [infoDismissed, setInfoDismissed] = useState((BrowserDataHelper.getLocalStorage(MemberRoleConstants._ContentFolderInfoDismissedKey) ??  "false") === "true");
    const [securitySummary, setSecuritySummary] = useState(memberRolePermissionSummary);

    const { execute: submit } = usePageCommand<ContentFolderMemberRoleData, ContentFolderMemberRoleData>(
        Commands.SetProperties,
        {
            data: { breakInheritance: breakInheritanceVal, requireAuthentication : requireAuthenticationVal, memberRoleTagIds: selectedRoleTagIds, memberRolePermissionSummary: securitySummary },
            after: (response) => {
                if (response) {
                    setSelectedRoleTagIds(response.memberRoleTagIds);
                    setBreakInheritance(response.breakInheritance);
                    setRequireAuthentication(response.requireAuthentication);
                    setSecuritySummary(response.memberRolePermissionSummary);
                }
            },
        }
    );

    var toggleCategoryCheck = function (event: React.ChangeEvent<HTMLInputElement>, checked: Boolean): void {
        if (checked) {
            setSelectedRoleTagIds([...selectedRoleTagIds, parseInt(event.target.name)])
        } else {
            setSelectedRoleTagIds(selectedRoleTagIds.filter(function (item) {
                return item != parseInt(event.target.name);
            }));
        }
    }
    var dismissWarning = function() {
        BrowserDataHelper.setLocalStorage(MemberRoleConstants._NoWorkflowWarningDismissedKey, "true", 30);
        setWarningDismissed(true);
    }
    var dismissInfo = function() {
        BrowserDataHelper.setLocalStorage(MemberRoleConstants._ContentFolderInfoDismissedKey, "true", 30);
        setInfoDismissed(true);
    }

    return (
        <div>
            <Paper fullHeight>
                <Box spacing={Spacing.XXL} >
                
                
                        <Headline size={HeadlineSize.L} spacingTop={Spacing.Micro} spacingBottom={Spacing.S}>Member Security (Content Folder)</Headline>
                        {!warningDismissed && 
                            <>
                                <NotificationBarWarning onDismiss={dismissWarning}>
                                These settings are not part of any workflow or language.  Changes are immediate.
                                </NotificationBarWarning><br/>
                            </>
                        }
                        {!infoDismissed && 
                            <>
                                <NotificationBarInfo onDismiss={dismissInfo}>
                                To set security for members, break the inheritance and adjust your settings.  Any Content Items below this Content Folder that have the 'Override Inherited Member Permissions' set to false on the Content tab will honor these permissions. 
                                </NotificationBarInfo><br/>
                            </>
                        }
                        

                        <MemberRolesPermissionSummaryComponent 
                            inheritingFrom={securitySummary.inheritingFrom}
                            requiresAuthentication={securitySummary.requiresAuthentication}
                            memberRoles={securitySummary.memberRoles} 
                            editLink={securitySummary.editLink}
                        />
                        <Checkbox label="Break Inheritance" size={CheckboxSize.L} name="breakInheritance" checked={breakInheritanceVal} onChange={(event) => setBreakInheritance(event.target.checked)}/>
                        {breakInheritanceVal && 
                        <>
                            <br/>
                            <Checkbox label="Is Secure (Requires Authentication)" size={CheckboxSize.L} name="requireAuthentication" checked={requireAuthenticationVal} onChange={(event) => setRequireAuthentication(event.target.checked)}/>
                            {requireAuthenticationVal && 
                            <>
                            <br/>
                            <Headline spacingTop={Spacing.XL} size={HeadlineSize.M} >Member Roles</Headline>
                            <ul style={{ paddingLeft: 0, marginLeft: 0, color: "black", listStyle: "none" }}>
                                {availableRoles.map((category, i) => {
                                    return (
                                        <li key={category.tagName}>
                                            <Checkbox label={category.tagTitle} size={CheckboxSize.L} name={category.tagId.toString()} checked={(selectedRoleTagIds.indexOf(category.tagId) > -1)} onChange={toggleCategoryCheck} />
                                        </li>
                                    )
                                })}
                            </ul>
                            </>
                            }
                            
                        </>
                        }

                        
                        <br/><br/>
                        <Button onClick={() => submit()} color={ButtonColor.Primary} label="Save Changes" />
                </Box>
            </Paper>
        </div>
    );
};