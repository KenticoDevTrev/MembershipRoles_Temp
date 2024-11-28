import React from "react";

export const RedirectComponent = ({ redirectUrl }: RedirectActionComponentClientProperties) => {
    var redirect = () => {
        window.location.assign(redirectUrl);
    };
    return (
        <>
        {redirect()}
        </>
    );
}

interface RedirectActionComponentClientProperties {
    redirectUrl: string
}
