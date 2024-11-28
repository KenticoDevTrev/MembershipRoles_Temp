import { LinkType } from "@kentico/xperience-admin-base";
import { Box, ButtonColor, ButtonSize, LinkButton, Spacing } from "@kentico/xperience-admin-components";
import React from "react";
export const MemberRolesPermissionSummaryComponent = (props: MemberRolesPermissionSummary) => {
    
    return (
        <>
        <Box spacingBottom={Spacing.M} className="card-text">
            <div style={{color:"var(--color-text-default-on-light)"}}>
                <strong>Inheriting From: </strong>
                {props.inheritingFrom} {props.editLink && 
                    <LinkButton title="Go to Inherited Ancestor" color={ButtonColor.Quinary} size={ButtonSize.XS} icon={'xp-arrow-right-top-square'} href={props.editLink} target={"_blank"}/>
                }
                <br/>
                
                    
                <strong>Current Permissions: </strong>{props.requiresAuthentication ? `Requires Authentication ${(props.memberRoles.length > 0 ? '+ in one of these roles: ['+props.memberRoles.join(', ')+']' : '')}` : 'Does not Require Authentication'}<br/>
                </div>
        </Box>
        </>
        
    )
}

export interface MemberRolesPermissionSummary {
    inheritingFrom : string,
    requiresAuthentication : boolean,
    memberRoles : Array<string>,
    editLink: string
}
